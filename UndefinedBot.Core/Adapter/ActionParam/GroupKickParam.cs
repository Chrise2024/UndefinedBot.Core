namespace UndefinedBot.Core.Adapter.ActionParam;

public sealed class GroupKickParam : IActionParam
{
    public required bool PermanentReject;
    public required string UserId;
}