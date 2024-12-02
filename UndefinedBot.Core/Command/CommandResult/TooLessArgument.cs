namespace UndefinedBot.Core.Command.CommandResult
{
    public class TooLessArgument(IEnumerable<string> requiredType) : ICommandResult
    {
        public ExecuteStatus Status => ExecuteStatus.NullArgument;
        public IEnumerable<string> RequiredType => requiredType;
    }
}
