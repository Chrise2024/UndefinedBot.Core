namespace UndefinedBot.Core.Adapter.ActionParam;

public class RecallMsgParam : IActionParam
{
    public required string MsgId { get; init; }
}