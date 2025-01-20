namespace UndefinedBot.Core.Adapter.ActionParam;

public class SendPrivateMsgParam : IActionParam
{
    //Temporary solution, will be replaced by message chain
    public required string Message { get; init; }
}