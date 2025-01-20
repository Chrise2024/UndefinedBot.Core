namespace UndefinedBot.Core.Adapter.ActionParam;

public class GetGroupMemberInfoParam : IActionParam
{
    public required string UserId { get; init; }
}