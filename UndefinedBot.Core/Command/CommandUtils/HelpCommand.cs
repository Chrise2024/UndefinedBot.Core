using System.Text.Json;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Command.CommandNode;
using UndefinedBot.Core.Command.CommandSource;

namespace UndefinedBot.Core.Command.CommandUtils;

internal sealed class HelpCommand
{
    private readonly Dictionary<string, List<CommandInstance>> _commandIndex;
    private readonly CommandInstance _instance = new("help", "core", ["all"]);

    public HelpCommand(Dictionary<string, List<CommandInstance>> commandIndex)
    {
        _commandIndex = commandIndex;
        _instance.Execute(async (ctx, _) =>
        {
            await ctx.SendFeedbackAsync(GenerateGeneralHelpText(ctx.Information));
        })
        .Then(new VariableNode("cmd", new StringArgument())
            .Execute(async (ctx, _) =>
            {
                string cmd = StringArgument.GetString("cmd", ctx);
                string text = GenerateHelpText(ctx.Information, cmd) ?? "咦，没有这个指令";
                await ctx.SendFeedbackAsync(text);
                ctx.Logger.Warn($"Command Not Found: <{cmd}>");
            })
        );
    }
    public async void InvokeHelpCommandAsync(CommandInformation information, BaseCommandSource source)
    {
        Console.WriteLine(JsonSerializer.Serialize(_commandIndex));
        CommandContext ctx = new(_instance,information);
        await _instance.RunAsync(ctx, source,information.Tokens);
    }

    private string GenerateGeneralHelpText(CommandInformation information)
    {
        //Must be submitted by an exist adapter
        string text = (_commandIndex.TryGetValue(information.AdapterId, out var v) ? v : [])
            .Where(c => !c.IsHidden())
            .Aggregate("",
                (current, p) =>
                    current + p.GetShortHelpText(information)
            );
        return "---------------help---------------\n指令列表：\n" +
               text +
               $"使用#help+具体指令查看使用方法\ne.g. {information.CommandPrefix}help help";
    }

    private string? GenerateHelpText(CommandInformation information,string commandName)
    {
        CommandInstance? command = _commandIndex[information.AdapterId].Find(x => x.IsTargetCommandLiteral(information,commandName));
        return command?.GetFullHelpText(information);
    }
}