namespace UndefinedBot.Core.Command.CommandResult;

internal sealed class InvalidArgumentCommandResult(string errorToken, string[] requiredType) : ICommandResult
{
    public ExecuteStatus Status => ExecuteStatus.InvalidArgument;
    public string ErrorToken => errorToken;
    public string[] RequiredType => requiredType;
}