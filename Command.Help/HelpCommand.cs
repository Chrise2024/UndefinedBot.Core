﻿using Newtonsoft.Json;
using UndefinedBot.Core;
using UndefinedBot.Core.Command;

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
                .Description("指令帮助文档")
                .ShortDescription("帮助")
                .Usage("{0}help [指令名]")
                .Example("{0}help help")
                .Action(async(args) =>
                {
                    if (_commandReference.Count == 0)
                    {
                        _commandReference = JsonConvert.DeserializeObject<Dictionary<string, CommandInstance>>(File.ReadAllText(Path.Join(_undefinedApi.RootPath,"command_reference.json"))) ?? [];
                    }
                    if (args.Param.Count > 0)
                    {
                        if (_commandReference.TryGetValue(args.Param[0], out var prop))
                        {
                            string? desc = prop.CommandDescription;
                            string? ug = prop.CommandUsage;
                            string? eg = prop.CommandExample;
                            if (desc != null || eg != null || ug != null)
                            {
                                await _undefinedApi.Api.SendGroupMsg(
                                    args.GroupId,
                                    _undefinedApi.GetMessageBuilder()
                                        .Text("---------------help---------------\n" + (desc == null ? "" : $"{prop.Name} - {desc}\n") + (ug == null ? "" : $"使用方法: \n{string.Format(ug,_commandPrefix)}\n") + (eg == null ? "" : $"e.g.\n{string.Format(eg,_commandPrefix)}\n") + $"可用指令别名: \n{JsonConvert.SerializeObject(prop.CommandAlias)}").Build()
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
                            string text = "";
                            foreach (var pair in _commandReference)
                            {
                                text += $"{_commandPrefix}{pair.Value.Name} - {pair.Value.CommandShortDescription ?? ""}\n";
                            }
                            _baseHelpText = "---------------help---------------\n指令列表：\n" +
                                text +
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
