namespace UndefinedBot.Core.Adapter.ActionParam;

public class GroupMuteParam : IActionParam
{
    public required TimeSpan Duration { get; init; }
    public required string UserId { get; init; }
}