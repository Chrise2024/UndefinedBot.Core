using UndefinedBot.Core.Command.Arguments.ArgumentType;

namespace UndefinedBot.Core.Command.CommandNodes
{
    public delegate Task CommandNodeAction(CommandContext ctx);
    //public delegate ICommandNode RegisterNodeAction(ICommandNode parentNode);
    public interface ICommandNode
    {
        public string NodeName { get; }
        internal IArgumentType ArgumentType { get; }
        internal ICommandNode? Parent { get; }
        internal List<ICommandNode> Child { get; }
        internal CommandNodeAction? NodeAction { get; }
        internal void SetParent(ICommandNode parentNode);
        internal void SetAction(CommandNodeAction action);
        public ICommandNode Then(ICommandNode nextNode);
        public ICommandNode Execute(CommandNodeAction action);
        internal Task<ExecuteStatus> ExecuteSelf(CommandContext ctx,List<string> tokens);
    }
    public class CommandAbortException(string? message = null) : Exception(message);

    public class InvalidArgumentException(string currentToken, string requiredType) : Exception(null)
    {
        public string ErrorToken => currentToken;
        public string RequiredType => requiredType;
    }
    public class TooLessArgumentException() : Exception();

    public class PermissionDeniedException(string currentNode, string currentPermission, string requiredPermission) : Exception(null)
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
}


