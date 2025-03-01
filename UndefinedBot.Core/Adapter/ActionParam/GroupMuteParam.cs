namespace UndefinedBot.Core.Adapter.ActionParam;

public sealed class GroupMuteParam : IActionParam
{
    public required TimeSpan Duration;
    public required string UserId;
}