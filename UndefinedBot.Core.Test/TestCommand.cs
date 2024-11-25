using Newtonsoft.Json;
using UndefinedBot.Core;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments.ArgumentType;

namespace UndefinedBot.Core.Test
{
    public class TestCommand
    {
        private readonly UndefinedAPI _undefinedApi;
        private readonly string _pluginName;

        public TestCommand(string pluginName)
        {
            _undefinedApi = new(pluginName);
            _pluginName = pluginName;
            _undefinedApi.RegisterCommand("test")
                .Description("指令帮助文档")
                .ShortDescription("帮助")
                .Usage("{0}help [指令名]")
                .Example("{0}help help")
                .Execute(async (ctx) =>
                {
                    Console.WriteLine(JsonConvert.SerializeObject(ctx.ArgumentReference));
                }).Then(new VariableNode("sub1", new PosIntArgument())
                    .Execute(async (ctx) =>
                    {
                        Console.WriteLine(JsonConvert.SerializeObject(ctx.ArgumentReference));
                    }));
            _undefinedApi.SubmitCommand();
        }
    }
}


