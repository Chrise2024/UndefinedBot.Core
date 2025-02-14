using System.Text.Json;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Adapter.ActionParam;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.Arguments.TokenContentType;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Command.CommandResult;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Plugin.BasicMessage;

namespace UndefinedBot.Core.Plugin;

internal static class CommandManager
{
    internal static Dictionary<string, CommandInstance[]> CommandInstanceIndexByAdapter { get; private set; } = [];

    public static async Task<CommandInvokeResult> InvokeCommand(CommandBackgroundEnvironment backgroundEnvironment,
        BaseCommandSource source)
    {
        if (backgroundEnvironment.CalledCommandName.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            HelpCommandHandler.HandleHelpCommand(backgroundEnvironment);
            return CommandInvokeResult.SuccessInvoke;
        }

        if (!CommandInstanceIndexByAdapter.TryGetValue(backgroundEnvironment.AdapterId, out CommandInstance[]? refCollection))
        {
            return CommandInvokeResult.NoCommandRelateToAdapter;
        }

        CommandInstance? targetCommand = Array.Find(refCollection, t => t.IsTargetCommand(backgroundEnvironment,source));
        if (targetCommand is null)
        {
            return CommandInvokeResult.NoSuchCommand;
        }

        if (targetCommand.IsReachRateLimit(backgroundEnvironment))
        {
            return CommandInvokeResult.CommandRateLimited;
        }

        CommandContext ctx = new(targetCommand, backgroundEnvironment);
        ctx.Logger.Info("Command Triggered");
        try
        {
            ICommandResult result = await targetCommand.Run(ctx, source, backgroundEnvironment.Tokens);
            switch (result)
            {
                case CommandSuccess:
                    //ignore
                    break;
                case InvalidArgumentCommandResult iae:
                    ctx.Logger.Error(
                        $"Invalid argument: {iae.ErrorToken}, require {JsonSerializer.Serialize(iae.RequiredType)}");
                    break;
                case TooLessArgument tae:
                    ctx.Logger.Error(
                        $"To less arguments, require {JsonSerializer.Serialize(tae.RequiredType)}");
                    break;
            }
        }
        catch (CommandAbortException)
        {
            ctx.Logger.Error($"Command Execute Aborted");
        }
        catch (CommandSyntaxException cse)
        {
            ctx.Logger.Error($"Node {cse.CurrentNode} Not Implemented");
        }
        catch (Exception ex)
        {
            ctx.Logger.Error(ex, "Command Failed");
        }

        ctx.Dispose();
        targetCommand.Cache.UpdateCache();
        return CommandInvokeResult.SuccessInvoke;
    }

    public static void UpdateCommandInstances(IEnumerable<CommandInstance> ci)
    {
        CommandInstanceIndexByAdapter.Clear();
        CommandInstanceIndexByAdapter = ci
            .GroupBy(
                i => i.TargetAdapterId,
                p => p
            )
            .ToDictionary(
                k => k.Key,
                v => v.ToArray()
            );
        GC.Collect();
    }

    public static void DisposeCommandInstance()
    {
        foreach (CommandInstance ci in CommandInstanceIndexByAdapter.SelectMany(pair => pair.Value))
        {
            ci.Dispose();
        }
    }
}

internal static class HelpCommandHandler
{
    private static string BaseHelpText { get; set; } = "";

    private static readonly string _commandHelpTextBase =
        "---------------help---------------\n{0}{1}{2}\n可用指令别名: \n{3}";

    private static Dictionary<string, CommandProperties[]> _commandReference = [];

    public static void HandleHelpCommand(CommandBackgroundEnvironment backgroundEnvironment)
    {
        HelpCommandContext ctx = new(backgroundEnvironment);
        ctx.Logger.Info("Help Command Triggered");
        if (_commandReference.Count == 0)
        {
            _commandReference = CommandManager.CommandInstanceIndexByAdapter.ToDictionary(
                k => k.Key,
                v => v.Value.Select(x =>
                    x.ExportToCommandProperties(ActionManager.AdapterInstanceReference)).ToArray()
            );
        }

        if (BaseHelpText.Length == 0)
        {
            CommandProperties[] commandCollection =
                _commandReference.TryGetValue(backgroundEnvironment.AdapterId, out var v) ? v : [];
            string text = commandCollection
                .Where(c => !c.IsHidden)
                .Aggregate("",
                    (current, p) =>
                        current +
                        $"{p.CommandPrefix}{p.Name} - {p.CommandShortDescription ?? ""}\n"
                );
            BaseHelpText = "---------------help---------------\n指令列表：\n" +
                           text +
                           $"使用#help+具体指令查看使用方法\ne.g. {backgroundEnvironment.CommandPrefix}help help";
        }

        if (backgroundEnvironment.Tokens.Length == 0 || backgroundEnvironment.Tokens[0] is not
                { TokenType: ParsedTokenTypes.Text, Content: TextTokenContent cmd })
        {
            ctx.Action.InvokeDefaultAction(DefaultActionType.SendGroupMsg,
                DefaultActionParameterWrapper.Common(backgroundEnvironment.SourceId,
                    new SendGroupMgsParam
                    {
                        MessageChain = [new TextMessageNode{Text = BaseHelpText}]
                    })
            );
            return;
        }

        //string cmd = Encoding.UTF8.GetString(invokeProperties.Tokens[0].Content);
        if (_commandReference.TryGetValue(backgroundEnvironment.AdapterId, out var cps) && cps.Length > 0)
        {
            CommandProperties cp = cps[0];
            string? desc = cp.CommandDescription;
            string? ug = cp.CommandUsage;
            string? eg = cp.CommandExample;
            string txt = string.Format(
                _commandHelpTextBase,
                [
                    desc == null ? "" : $"{cp.Name} - {desc}\n",
                    ug == null ? "" : $"使用方法: \n{string.Format(ug, cp.CommandPrefix)}\n",
                    eg == null ? "" : $"e.g.\n{string.Format(eg, cp.CommandPrefix)}\n",
                    JsonSerializer.Serialize(cp.CommandAlias)
                ]
            );
            ctx.Action.InvokeDefaultAction(DefaultActionType.SendGroupMsg,
                DefaultActionParameterWrapper.Common(backgroundEnvironment.SourceId,
                    new SendGroupMgsParam
                    {
                        MessageChain = [new TextMessageNode{Text = txt}]
                    })
            );
        }
        else
        {
            ctx.Action.InvokeDefaultAction(DefaultActionType.SendGroupMsg,
                DefaultActionParameterWrapper.Common(backgroundEnvironment.SourceId,
                    new SendGroupMgsParam
                    {
                        MessageChain = [new TextMessageNode{Text = "咦，没有这个指令"}]
                    })
            );
            ctx.Logger.Warn($"Command Not Found: <{cmd}>");
        }
    }
}

public enum CommandInvokeResult
{
    SuccessInvoke = 0,
    NoCommandRelateToAdapter = 1,
    NoSuchCommand = 2,
    CommandRateLimited = 3,
}