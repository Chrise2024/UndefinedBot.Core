using System.Text.Json;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Plugin;
using UndefinedBot.Core.Utils;

namespace Command.Help;

public sealed class HelpCommand(PluginConfigData pluginConfig) : BasePlugin(pluginConfig)
{
    public override string Id => "Help";
    public override string Name => "Help Plugin";
    public override string TargetAdapterId => "OneBot11Adapter";
    private string BaseHelpText { get; set; } = "";
    private string CommandPrefix => ConfigManager.GetCommandPrefix();

    private Dictionary<string, CommandProperties> _commandReference = [];
    public override void Initialize()
    {
        RegisterCommand("help")
            .Description("指令帮助文档")
            .ShortDescription("帮助")
            .Usage("{0}help [指令名]")
            .Example("{0}help help")
            .Execute(async (ctx,_) =>
            {
                if (_commandReference.Count == 0)
                {
                    _commandReference =
                        JsonSerializer.Deserialize<Dictionary<string,CommandProperties>>(
                            await File.ReadAllTextAsync(Path.Join(ctx.RootPath, "command_reference.json"))) ?? [];
                }

                if (BaseHelpText.Length == 0)
                {
                    string text = _commandReference.Aggregate("",
                        (current, pair) =>
                            current +
                            $"{CommandPrefix}{pair.Value.Name} - {pair.Value.CommandShortDescription ?? ""}\n");

                    BaseHelpText = "---------------help---------------\n指令列表：\n" +
                                    text +
                                    $"使用#help+具体指令查看使用方法\ne.g. {CommandPrefix}help help";
                }
                ctx.Action.InvokeDefaultAction(DefaultActionType.SendGroupMsg, []);
                /*await ctx.Api.SendGroupMsg(
                    ctx.InvokeProperties.SourceId,
                    ctx.GetMessageBuilder()
                        .Text(string.Format(BaseHelpText, CommandPrefix)).Build()
                );*/
            }).Then(new VariableNode("cmd", new StringArgument())
                .Execute(async (ctx,_) =>
                {
                    if (_commandReference.Count == 0)
                    {
                        _commandReference =
                            JsonSerializer.Deserialize<Dictionary<string,CommandProperties>>(
                                await File.ReadAllTextAsync(Path.Join(ctx.RootPath, "command_reference.json"))) ?? [];
                    }

                    string cmd = StringArgument.GetString("cmd", ctx);
                    if (_commandReference.TryGetValue(cmd, out CommandProperties? prop))
                    {
                        string? desc = prop.CommandDescription;
                        string? ug = prop.CommandUsage;
                        string? eg = prop.CommandExample;
                        if (desc != null || eg != null || ug != null)
                        {
                            ctx.Action.InvokeDefaultAction(DefaultActionType.SendGroupMsg, []);
                            /*await ctx.Api.SendGroupMsg(
                                ctx.InvokeProperties.SourceId,
                                ctx.GetMessageBuilder()
                                    .Text("---------------help---------------\n" +
                                          (desc == null ? "" : $"{prop.Name} - {desc}\n") +
                                          (ug == null ? "" : $"使用方法: \n{string.Format(ug, CommandPrefix)}\n") +
                                          (eg == null ? "" : $"e.g.\n{string.Format(eg, CommandPrefix)}\n") +
                                          $"可用指令别名: \n{JsonSerializer.Serialize(prop.CommandAlias)}").Build()
                            );*/
                        }
                    }
                    else
                    {
                        ctx.Action.InvokeDefaultAction(DefaultActionType.SendGroupMsg, []);
                        /*await ctx.Api.SendGroupMsg(
                            ctx.InvokeProperties.SourceId,
                            ctx.GetMessageBuilder()
                                .Text("咦，没有这个指令").Build()
                        );*/
                        ctx.Logger.Warn($"Command Not Found: <{cmd}>");
                    }
                })
            );
    }
}
