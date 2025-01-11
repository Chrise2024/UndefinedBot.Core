using System.Reflection;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Plugin;

namespace UndefinedBot.Core.Test;

public sealed class TestCommand : BasePlugin
{
    public override string Id => "Test";
    public override string Name => "Test Plugin";
    public override string TargetAdapterId => "OneBot11Adapter";
    public override void Initialize()
    {
        Console.WriteLine(PluginPath);
        RegisterCommand("test")
            .Description("指令帮助文档")
            .ShortDescription("帮助")
            .Usage("{0}help [指令名]")
            .Example("{0}help help")
            .Execute(async (ctx, source) => { Console.WriteLine("root"); })
            .Then(new VariableNode("var1", new ReplyArgument())
                .Then(new VariableNode("var2", new StringArgument())
                    .Execute(async (ctx, source) =>
                    {
                        long v1 = IntegerArgument.GetInteger("var1", ctx);
                        double v2 = NumberArgument.GetNumber("var2", ctx);
                        Console.WriteLine(v1 * v2);
                    })))
            .Then(new VariableNode("var3", new NumberArgument())
                //.Require((ip,s) => false)
                .Then(new VariableNode("var4", new IntegerArgument())
                    .Execute(async (ctx, source) =>
                    {
                        double v3 = NumberArgument.GetNumber("var3", ctx);
                        long v4 = IntegerArgument.GetInteger("var4", ctx);
                        Console.WriteLine(v3 + v4);
                    })));
    }
}