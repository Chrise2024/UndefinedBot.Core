using System.Text.Json;
using UndefinedBot.Core;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Command.Arguments.ArgumentType;

namespace Command.Help;

public class HelpCommand
{
    private readonly UndefinedApi _undefinedApi;

    private readonly string _pluginName;

    private string _baseHelpText = "";

    private readonly string _commandPrefix;

    private Dictionary<string, CommandProperty> _commandReference = [];

    public HelpCommand(string pluginName)
    {
        _undefinedApi = new(pluginName);
        _pluginName = pluginName;
        _commandPrefix = _undefinedApi.Config.CommandPrefix;
        _undefinedApi.RegisterCommand("help")
            .Description("指令帮助文档")
            .ShortDescription("帮助")
            .Usage("{0}help [指令名]")
            .Example("{0}help help")
            .Execute(async (ctx) =>
            {
                if (_commandReference.Count == 0)
                {
                    _commandReference =
                        JsonSerializer.Deserialize<Dictionary<string,CommandProperty>>(
                            File.ReadAllText(Path.Join(ctx.RootPath, "command_reference.json"))) ?? [];
                }

                if (_baseHelpText.Length == 0)
                {
                    string text = _commandReference.Aggregate("",
                        (current, pair) =>
                            current +
                            $"{_commandPrefix}{pair.Value.Name} - {pair.Value.CommandShortDescription ?? ""}\n");

                    _baseHelpText = "---------------help---------------\n指令列表：\n" +
                                    text +
                                    $"使用#help+具体指令查看使用方法\ne.g. {_commandPrefix}help help";
                }

                await ctx.Api.SendGroupMsg(
                    ctx.CallingProperties.GroupId,
                    ctx.GetMessageBuilder()
                        .Text(string.Format(_baseHelpText, _commandPrefix)).Build()
                );
            }).Then(new VariableNode("cmd", new StringArgument())
                .Execute(async (ctx) =>
                {
                    if (_commandReference.Count == 0)
                    {
                        _commandReference =
                            JsonSerializer.Deserialize<Dictionary<string,CommandProperty>>(
                                File.ReadAllText(Path.Join(ctx.RootPath, "command_reference.json"))) ?? [];
                    }

                    string cmd = StringArgument.GetString("cmd", ctx);
                    if (_commandReference.TryGetValue(cmd, out CommandProperty prop))
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
                                          (ug == null ? "" : $"使用方法: \n{string.Format(ug, _commandPrefix)}\n") +
                                          (eg == null ? "" : $"e.g.\n{string.Format(eg, _commandPrefix)}\n") +
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
                }));
    }
}
