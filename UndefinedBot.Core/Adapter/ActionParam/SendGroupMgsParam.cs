namespace UndefinedBot.Core.Adapter.ActionParam;

public sealed class SendGroupMgsParam : IActionParam
{
    //Temporary solution, will be replaced by message chain
    public required string Message { get; init; }
}