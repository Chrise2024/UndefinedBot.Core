namespace UndefinedBot.Core.Command.CommandException;

public class CommandSyntaxException(string currentNode) : Exception(null)
{
    public string CurrentNode => currentNode;
}