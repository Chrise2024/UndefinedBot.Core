namespace UndefinedBot.Core.Command.CommandResult;

public class InvalidArgument(string errorToken, IEnumerable<string> requiredType) : ICommandResult
{
    public ExecuteStatus Status => ExecuteStatus.InvalidArgument;
    public string ErrorToken => errorToken;
    public string[] RequiredType => requiredType.ToArray();
}