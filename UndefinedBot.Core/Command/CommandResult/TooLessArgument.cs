namespace UndefinedBot.Core.Command.CommandResult;

public class TooLessArgument(IEnumerable<string> requiredType) : ICommandResult
{
    public ExecuteStatus Status => ExecuteStatus.NullArgument;
    public string[] RequiredType => requiredType.ToArray();
}