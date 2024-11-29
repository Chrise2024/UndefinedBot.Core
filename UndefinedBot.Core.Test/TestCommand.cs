using Newtonsoft.Json;
using UndefinedBot.Core;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Command.Arguments.ArgumentType;

namespace UndefinedBot.Core.Test
{
    public class TestCommand
    {
        private readonly UndefinedApi _undefinedApi;
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
                    Console.WriteLine("root");
                    Console.WriteLine(JsonConvert.SerializeObject(ctx.ArgumentReference));
                }).Then(new SubCommandNode("sub1")
                    .Execute(async (ctx) =>
                    {
                        Console.WriteLine("s1");
                        Console.WriteLine(JsonConvert.SerializeObject(ctx.ArgumentReference));
                    }))
                .Then(new SubCommandNode("sub2")
                    .Execute(async (ctx) =>
                    {
                        Console.WriteLine("s2");
                        Console.WriteLine(JsonConvert.SerializeObject(ctx.ArgumentReference));
                    }).Then(new VariableNode("var1",new IntegerArgument())
                        .Execute(async (ctx) =>
                        {
                            Console.WriteLine("s2-v1");
                            Console.WriteLine(JsonConvert.SerializeObject(ctx.ArgumentReference));
                        }).Then(new VariableNode("val2",new PosIntArgument())
                            .Execute(async (ctx) =>
                            {
                                Console.WriteLine("s2-v2");
                                Console.WriteLine(JsonConvert.SerializeObject(ctx.ArgumentReference));
                            }))));
            _undefinedApi.SubmitCommand();
        }
    }
}


