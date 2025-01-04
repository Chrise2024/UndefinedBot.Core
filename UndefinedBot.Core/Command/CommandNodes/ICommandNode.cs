using UndefinedBot.Core.Command.CommandResult;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Command.CommandSource;

namespace UndefinedBot.Core.Command.CommandNodes;

[Obsolete("wasted", true)]
public delegate Task CommandNodeAction(CommandContext ctx);

//public delegate ICommandNode RegisterNodeAction(ICommandNode parentNode);
public interface ICommandNode
{
    public string NodeName { get; }
    internal IArgumentType ArgumentType { get; }
    internal ICommandNode? Parent { get; }
    internal List<ICommandNode> Child { get; }
    internal Func<CommandContext, BaseCommandSource, Task>? NodeAction { get; }
    internal void SetParent(ICommandNode parentNode);
    internal void SetAction(Func<CommandContext, BaseCommandSource, Task> action);
    public ICommandNode Then(ICommandNode nextNode);
    public ICommandNode Execute(Func<CommandContext, BaseCommandSource, Task> action);
    internal Task<ICommandResult> ExecuteSelf(CommandContext ctx, BaseCommandSource source, List<ParsedToken> tokens);
    public string GetArgumentRequire();
}

public class CommandAbortException(string? message = null) : Exception(message);

[Obsolete("wasted", true)]
public class InvalidArgumentException(string currentToken, List<string> requiredType) : Exception(null)
{
    public string ErrorToken => currentToken;
    public List<string> RequiredType => requiredType;
}

[Obsolete("wasted", true)]
public class TooLessArgumentException(List<string> requiredType) : Exception(null)
{
    public List<string> RequiredType => requiredType;
}

[Obsolete("wasted", true)]
public class PermissionDeniedException(string currentNode, string currentPermission, string requiredPermission)
    : Exception(null)
{
    public string CurrentNode => currentNode;
    public string CurrentPermission => currentPermission;
    public string RequiredPermission => requiredPermission;
}

public class CommandSyntaxException(string currentNode) : Exception(null)
{
    public string CurrentNode => currentNode;
}
//public class CommandSuccessException(string? message = null) : Exception(message);