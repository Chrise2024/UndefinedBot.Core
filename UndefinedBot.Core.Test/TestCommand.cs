using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.Arguments.ArgumentRange;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Command.Arguments.TokenContentType;
using UndefinedBot.Core.Command.CommandNode;
using UndefinedBot.Core.Command.CommandUtils;
using UndefinedBot.Core.Plugin;

namespace UndefinedBot.Core.Test;

public sealed class TestCommand : BasePlugin
{
    public override string Id => "Test";
    public override string Name => "Test Plugin";
    public override string[] TargetAdapter => ["OneBot11Adapter"];

    public override void Initialize()
    {
        //Console.WriteLine(PluginPath);
        RegisterCommand("test")
            .Description("指令帮助文档")
            .ShortDescription("帮助")
            .Usage("{0}help [指令名]")
            .Example("{0}help help")
            .Execute(async delegate { Console.WriteLine("root"); })
            .Then(new VariableNode("var1", new ReplyArgument())
                .Then(new VariableNode("var2", new StringArgument())
                    .Execute(async (ctx, _, _) =>
                    {
                        long v1 = IntegerArgument.GetInteger("var1", ctx);
                        double v2 = NumberArgument.GetNumber("var2", ctx);
                        Console.WriteLine(v1 * v2);
                    })))
            .Then(new VariableNode("var3", new NumberArgument())
                //.Require((ip,s) => false)
                .Then(new VariableNode("var4", new IntegerArgument())
                    .Execute(async (ctx, _, _) =>
                    {
                        double v3 = NumberArgument.GetNumber("var3", ctx);
                        long v4 = IntegerArgument.GetInteger("var4", ctx);
                        ctx.Logger.Info($"{v3} + {v4} = {v3 + v4}");
                    })));
    }
}

internal sealed class MyArgument : CustomArgument
{
    public override string ArgumentTypeName => "MyArgument";
    public override IArgumentRange? Range => null;

    protected override bool ValidContent(CustomTokenContent content)
    {
        return content is MyTokenContent { Text: { Length: > 0 } };
    }
    public static object GetObject(string key, CommandContext ctx)
    {
        return GetExactTypeValue(ctx.GetArgumentReference(key));
    }
    
    private static object GetExactTypeValue(ParsedToken token)
    {
        return new object();
    }
}

internal sealed class MyTokenContent(string text) : CustomTokenContent
{
    public string Text { get; } = text;
}