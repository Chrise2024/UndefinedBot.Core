namespace UndefinedBot.Core.Adapter.ActionParam;

public sealed class GroupKickParam : IDefaultActionParam
{
    public required bool PermanentReject;
    public required string UserId;
}