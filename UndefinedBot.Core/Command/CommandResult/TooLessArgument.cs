namespace UndefinedBot.Core.Command.CommandResult;

internal sealed class TooLessArgument(List<string> requiredType) : ICommandResult
{
    public ExecuteStatus Status => ExecuteStatus.NullArgument;
    public string[] RequiredType => requiredType.ToArray();
}