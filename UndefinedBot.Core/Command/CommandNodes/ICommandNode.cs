using UndefinedBot.Core.Command.CommandResult;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Command.CommandSource;

namespace UndefinedBot.Core.Command.CommandNodes;

//public delegate ICommandNode RegisterNodeAction(ICommandNode parentNode);
public interface ICommandNode : IDisposable
{
    public string NodeName { get; }
    public CommandAttribFlags CommandAttrib { get; }
    internal IArgumentType ArgumentType { get; }
    internal ICommandNode? Parent { get; }
    internal List<ICommandNode> Child { get; }
    internal Func<CommandContext, BaseCommandSource, Task>? NodeAction { get; }
    internal Func<CommandBackgroundEnvironment,BaseCommandSource,bool>? NodeRequire { get; }
    internal void SetParent(ICommandNode parentNode);
    internal void SetCommandAttrib(CommandAttribFlags attr);
    internal void SetAction(Func<CommandContext, BaseCommandSource, Task> action);
    public ICommandNode Then(ICommandNode nextNode);
    public ICommandNode Require(Func<CommandBackgroundEnvironment,BaseCommandSource,bool> predicate);
    public ICommandNode Execute(Func<CommandContext, BaseCommandSource, Task> action);
    internal Task<ICommandResult> ExecuteSelfAsyncAsync(CommandContext ctx, BaseCommandSource source, ParsedToken[] tokens);
    public string GetArgumentRequire();
}

public class CommandAbortException(string? message = null) : Exception(message);
public class CommandSyntaxException(string currentNode) : Exception(null)
{
    public string CurrentNode => currentNode;
}