using System.Text.Json;
using UndefinedBot.Core;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Registry;
using UndefinedBot.Core.Utils;

namespace Command.Help;

public class HelpCommand : IPluginInitializer
{
    private string BaseHelpText { get; set; } = "";
    private string CommandPrefix => ConfigManager.GetCommandPrefix();

    private Dictionary<string, CommandMetaProperties> _commandReference = [];

    public void Initialize(UndefinedApi undefinedApi)
    {
        undefinedApi.RegisterCommand("help")
            .Description("指令帮助文档")
            .ShortDescription("帮助")
            .Usage("{0}help [指令名]")
            .Example("{0}help help")
            .Execute(async (ctx) =>
            {
                if (_commandReference.Count == 0)
                {
                    _commandReference =
                        JsonSerializer.Deserialize<Dictionary<string,CommandMetaProperties>>(
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

                await ctx.Api.SendGroupMsg(
                    ctx.CallingProperties.GroupId,
                    ctx.GetMessageBuilder()
                        .Text(string.Format(BaseHelpText, CommandPrefix)).Build()
                );
            }).Then(new VariableNode("cmd", new StringArgument())
                .Execute(async (ctx) =>
                {
                    if (_commandReference.Count == 0)
                    {
                        _commandReference =
                            JsonSerializer.Deserialize<Dictionary<string,CommandMetaProperties>>(
                                await File.ReadAllTextAsync(Path.Join(ctx.RootPath, "command_reference.json"))) ?? [];
                    }

                    string cmd = StringArgument.GetString("cmd", ctx);
                    if (_commandReference.TryGetValue(cmd, out CommandMetaProperties? prop))
                    {
                        string? desc = prop.CommandDescription;
                        string? ug = prop.CommandUsage;
                        string? eg = prop.CommandExample;
                        if (desc != null || eg != null || ug != null)
                        {
                            await ctx.Api.SendGroupMsg(
                                ctx.CallingProperties.GroupId,
                                ctx.GetMessageBuilder()
                                    .Text("---------------help---------------\n" +
                                          (desc == null ? "" : $"{prop.Name} - {desc}\n") +
                                          (ug == null ? "" : $"使用方法: \n{string.Format(ug, CommandPrefix)}\n") +
                                          (eg == null ? "" : $"e.g.\n{string.Format(eg, CommandPrefix)}\n") +
                                          $"可用指令别名: \n{JsonSerializer.Serialize(prop.CommandAlias)}").Build()
                            );
                        }
                    }
                    else
                    {
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Text("咦，没有这个指令").Build()
                        );
                        ctx.Logger.Warn($"Command Not Found: <{cmd}>");
                    }
                })
            );
    }
}
