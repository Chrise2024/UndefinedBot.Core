using UndefinedBot.Core.BasicMessage;

namespace UndefinedBot.Core.Adapter.ActionParam;

public sealed class SendGroupMgsParam : IActionParam
{
    //Temporary solution, will be replaced by message chain
    public required List<IMessageNode> MessageChain { get; init; }
}