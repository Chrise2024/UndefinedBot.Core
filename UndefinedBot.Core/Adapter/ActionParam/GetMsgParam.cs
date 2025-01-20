namespace UndefinedBot.Core.Adapter.ActionParam;

public class GetMsgParam : IActionParam
{
    public required string MsgId { get; init; }
}