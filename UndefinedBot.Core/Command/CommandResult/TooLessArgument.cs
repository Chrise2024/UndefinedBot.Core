namespace UndefinedBot.Core.Command.CommandResult;

public sealed class TooLessArgument(List<string> requiredType) : ICommandResult
{
    public ExecuteStatus Status => ExecuteStatus.NullArgument;
    public string[] RequiredType => requiredType.ToArray();
}
