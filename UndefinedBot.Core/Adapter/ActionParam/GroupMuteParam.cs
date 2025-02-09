namespace UndefinedBot.Core.Adapter.ActionParam;

public sealed class GroupMuteParam : IDefaultActionParam
{
    public required TimeSpan Duration;
    public required string UserId;
}