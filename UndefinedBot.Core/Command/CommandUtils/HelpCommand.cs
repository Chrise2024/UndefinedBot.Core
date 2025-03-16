using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Command.CommandNode;
using UndefinedBot.Core.Message;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Command.CommandUtils;

internal sealed class HelpCommand : IDisposable
{
    private readonly List<CommandInstance> _commandInstances;
    private readonly CommandInstance _instance;
    private readonly ActionManager _actionManager;

    public HelpCommand(List<CommandInstance> commandInstances, ActionManager actionManager,ILogger logger)
    {
        _commandInstances = commandInstances;
        _actionManager = actionManager;
        _instance = new CommandInstance("help", "core", ["all"], logger);
        _instance.Execute(async (ctx, _, _) =>
            {
                await ctx.SendFeedbackAsync(GenerateGeneralHelpText(ctx.Content));
            })
            .Then(new VariableNode("cmd", new StringArgument())
                .Execute(async (ctx, _, _) =>
                {
                    string cmd = StringArgument.GetString("cmd", ctx);
                    string? text = GenerateHelpText(ctx.Content, cmd);
                    if (text is null)
                    {
                        await ctx.SendFeedbackAsync("咦，没有这个指令");
                        ctx.Logger.Warn($"Command not found: <{cmd}>");
                        return;
                    }

                    await ctx.SendFeedbackAsync(text);
                })
            );
    }

    public async void InvokeHelpCommandAsync(CommandContent content, BaseMessageSource source)
    {
        //Console.WriteLine(JsonSerializer.Serialize(_commandIndex));
        CommandContext ctx = new(_instance, content, _actionManager);
        await _instance.RunAsync(ctx, source, content.Tokens);
        ctx.Dispose();
    }

    private string GenerateGeneralHelpText(CommandContent content)
    {
        //Must be submitted by an exist adapter
        string text = _commandInstances
            .Where(c => !c.IsHidden())
            .Aggregate("",
                (current, p) =>
                    current + p.GetShortHelpText(content)
            );
        return "---------------help---------------\n指令列表：\n" +
               text +
               $"使用#help+具体指令查看使用方法\ne.g. {content.CommandPrefix}help help";
    }

    private string? GenerateHelpText(CommandContent content, string commandName)
    {
        CommandInstance? command = _commandInstances.Find(x => x.IsTargetCommandLiteral(content, commandName));
        return command?.GetFullHelpText(content);
    }

    public void Dispose()
    {
        _instance.Dispose();
        _commandInstances.Clear();
    }
}