using UndefinedBot.Core.BasicMessage;

namespace UndefinedBot.Core.Adapter.ActionParam;

public class SendPrivateMsgParam : IActionParam
{
    //Temporary solution, will be replaced by message chain
    public required List<IMessageNode> MessageChain { get; init; }
}