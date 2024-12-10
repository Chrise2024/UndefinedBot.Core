using System.Text.Json;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Command.Arguments.ArgumentRange;
using UndefinedBot.Core.Command.Arguments.ArgumentType;

namespace UndefinedBot.Core.Test;

[Plugin]
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
            .Execute(async (ctx) => { Console.WriteLine("root"); }).Then(new VariableNode("var1", new IntegerArgument())
                .Then(new VariableNode("var2", new NumberArgument(new RangeBetween<double>(127, 255)))
                    .Execute(async (ctx) =>
                    {
                        long v1 = IntegerArgument.GetInteger("var1", ctx);
                        double v2 = NumberArgument.GetNumber("var2", ctx);
                        Console.WriteLine(v1 * v2);
                    })))
            .Then(new VariableNode("var3", new IntegerArgument())
                .Then(new VariableNode("var4", new IntegerArgument())
                    .Execute(async (ctx) =>
                    {
                        long v3 = IntegerArgument.GetInteger("var3", ctx);
                        long v4 = IntegerArgument.GetInteger("var4", ctx);
                        Console.WriteLine(v3 + v4);
                    })));
        _undefinedApi.SubmitCommand();
    }
}
