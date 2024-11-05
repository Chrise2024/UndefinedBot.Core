using System.Reflection;
using Newtonsoft.Json;
using UndefinedBot.Core;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Utils;

namespace Command.Help
{

    public class HelpCommand
    {
        private readonly UndefinedAPI _undefinedApi;

        private readonly string _pluginName;

        private string _baseHelpText = "";

        private readonly string _commandPrefix;

        private Dictionary<string, CommandInstance> _commandReference = [];
        public HelpCommand(string pluginName)
        {
            _undefinedApi = new(pluginName);
            _pluginName = pluginName;
            _commandPrefix = _undefinedApi.Config.GetCommandPrefix();
            _undefinedApi.RegisterCommand("help")
                .Description("{0}help - 帮助\n使用方法：{0}help")
                .ShortDescription("{0}help - 帮助")
                .Example("{0}help help")
                .Action(async(ArgSchematics args) =>
                {
                    if (_commandReference.Count == 0)
                    {
                        _commandReference = JsonConvert.DeserializeObject<Dictionary<string, CommandInstance>>(File.ReadAllText(Path.Join(_undefinedApi.RootPath,"command_reference.json"))) ?? [];
                    }
                    if (args.Param.Count > 0)
                    {
                        if (_commandReference.TryGetValue(args.Param[0], out var Prop))
                        {
                            string? desc = Prop.CommandDescription;
                            string? eg = Prop.CommandExample;
                            if (desc != null || eg != null)
                            {
                                await _undefinedApi.Api.SendGroupMsg(
                                    args.GroupId,
                                    _undefinedApi.GetMessageBuilder()
                                        .Text(string.Format("---------------help---------------\n" + (desc ?? "") + (eg == null ? "" : $"\ne.g.\n{eg}") + $"\n可用指令别名: {JsonConvert.SerializeObject(Prop.CommandAlias)}", _commandPrefix)).Build()
                                );
                            }
                        }
                        else
                        {
                            await _undefinedApi.Api.SendGroupMsg(
                                args.GroupId,
                                _undefinedApi.GetMessageBuilder()
                                    .Text("咦，没有这个指令").Build()
                            );
                            _undefinedApi.Logger.Warn("help",$"Command Not Found: <{args.Param[0]}>");
                        }
                    }
                    else
                    {
                        if (_baseHelpText.Length == 0)
                        {
                            string CommandText = "";
                            foreach (var pair in _commandReference)
                            {
                                CommandText += (pair.Value.CommandShortDescription ?? "") + "\n";
                            }
                            _baseHelpText = "---------------help---------------\n指令列表：\n" +
                                CommandText +
                                "使用#help+具体指令查看使用方法\ne.g. #help help";
                        }
                        await _undefinedApi.Api.SendGroupMsg(
                                    args.GroupId,
                                    _undefinedApi.GetMessageBuilder()
                                        .Text(string.Format(_baseHelpText, _commandPrefix)).Build()
                                );
                    }
                });
            _undefinedApi.SubmitCommand();
        }
    }
}
