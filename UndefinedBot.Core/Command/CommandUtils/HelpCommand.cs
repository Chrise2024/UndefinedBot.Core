using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Command.CommandNode;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Utils.Logging;

namespace UndefinedBot.Core.Command.CommandUtils;

internal sealed class HelpCommand : IDisposable
{
    private readonly List<CommandInstance> _commandInstances;
    private readonly CommandInstance _instance;
    private readonly IActionManager _actionManager;
    private readonly ILogger _logger;

    public HelpCommand(List<CommandInstance> commandInstances,IActionManager actionManager,ILogger logger)
    {
        _commandInstances = commandInstances;
        _actionManager = actionManager;
        _logger = logger;
        _instance = new("help", "core", ["all"],logger);
        _instance.Execute(async (ctx, _,_) =>
            {
                await ctx.SendFeedbackAsync(GenerateGeneralHelpText(ctx.Information));
            })
            .Then(new VariableNode("cmd", new StringArgument())
                .Execute(async (ctx, _,_) =>
                {
                    string cmd = StringArgument.GetString("cmd", ctx);
                    string? text = GenerateHelpText(ctx.Information, cmd);
                    if (text is null)
                    {
                        await ctx.SendFeedbackAsync("咦，没有这个指令");
                        ctx.Logger.Warn($"Command Not Found: <{cmd}>");
                        return;
                    }
                    await ctx.SendFeedbackAsync(text);
                })
            );
    }
    public async void InvokeHelpCommandAsync(CommandInformation information, BaseCommandSource source)
    {
        //Console.WriteLine(JsonSerializer.Serialize(_commandIndex));
        CommandContext ctx = new(_instance,information,_actionManager);
        await _instance.RunAsync(ctx, source,information.Tokens);
        ctx.Dispose();
    }

    private string GenerateGeneralHelpText(CommandInformation information)
    {
        //Must be submitted by an exist adapter
        string text = _commandInstances
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
        CommandInstance? command = _commandInstances.Find(x => x.IsTargetCommandLiteral(information,commandName));
        return command?.GetFullHelpText(information);
    }
    public void Dispose()
    {
        _instance.Dispose();
        _commandInstances.Clear();
    }
}