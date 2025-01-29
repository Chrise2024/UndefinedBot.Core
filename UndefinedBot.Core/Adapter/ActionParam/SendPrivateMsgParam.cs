using UndefinedBot.Core.Plugin.BasicMessage;

namespace UndefinedBot.Core.Adapter.ActionParam;

public class SendPrivateMsgParam : IActionParam
{
    public required IMessageNode[] MessageChain;
}