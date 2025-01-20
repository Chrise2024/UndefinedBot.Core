namespace UndefinedBot.Core.Adapter.ActionParam;

public class GroupKickParam : IActionParam
{
    public required bool PermanentReject { get; init; }
    public required string UserId { get; init; }
}