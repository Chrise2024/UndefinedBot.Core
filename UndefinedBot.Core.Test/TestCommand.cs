using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Registry;

namespace UndefinedBot.Core.Test;

public class TestCommand : IPluginInitializer
{
    public void Initialize(UndefinedApi undefinedApi)
    {
        undefinedApi.RegisterCommand("test")
            .Description("指令帮助文档")
            .ShortDescription("帮助")
            .Usage("{0}help [指令名]")
            .Example("{0}help help")
            .Execute(async (ctx) => { Console.WriteLine("root"); })
            .Then(new VariableNode("var1", new ReplyArgument())
                .Then(new VariableNode("var2", new StringArgument())
                    .Execute(async (ctx) =>
                    {
                        //long v1 = IntegerArgument.GetInteger("var1", ctx);
                        //double v2 = NumberArgument.GetNumber("var2", ctx);
                        //Console.WriteLine(v1 * v2);
                        QReply r = ReplyArgument.GetQReply("var1", ctx);
                        string ptn = StringArgument.GetString("var2", ctx);
                        Console.WriteLine(ptn);
                    })))
            .Then(new VariableNode("var3", new StringArgument())
                .Then(new VariableNode("var4", new ImageArgument())
                    .Execute(async (ctx) =>
                    {
                        //long v3 = IntegerArgument.GetInteger("var3", ctx);
                        //long v4 = IntegerArgument.GetInteger("var4", ctx);
                        //Console.WriteLine(v3 + v4);
                        string ptn = StringArgument.GetString("var3", ctx);
                        QImage r = ImageArgument.GetImage("var4", ctx);
                        Console.WriteLine(ptn);
                    })));
        //UndefinedApi.SubmitCommand();
    }
}
