using UndefinedBot.Core.Command.CommandResult;
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
        internal Task<ICommandResult> ExecuteSelf(CommandContext ctx,List<string> tokens);
        public string GetArgumentRequire();
    }
    public class CommandAbortException(string? message = null) : Exception(message);
    [Obsolete("wasted",true)]
    public class InvalidArgumentException(string currentToken, IEnumerable<string> requiredType) : Exception(null)
    {
        public string ErrorToken => currentToken;
        public IEnumerable<string> RequiredType => requiredType;
    }
    [Obsolete("wasted",true)]
    public class TooLessArgumentException(IEnumerable<string> requiredType) : Exception(null)
    {
        public IEnumerable<string> RequiredType => requiredType;
    }
    [Obsolete("wasted",true)]
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


