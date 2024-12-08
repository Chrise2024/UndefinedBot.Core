namespace UndefinedBot.Core.Command.CommandResult;

public class InvalidSyntax(string currentNode) : ICommandResult
{
    public ExecuteStatus Status => ExecuteStatus.InvalidSyntax;
    public string CurrentNode => currentNode;
}