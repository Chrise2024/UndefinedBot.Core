using System.Diagnostics.CodeAnalysis;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Command.CommandException;
using UndefinedBot.Core.Command.CommandResult;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Command.CommandUtils;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Command.CommandNode;

public abstract class CommandNode(string name, IArgumentType argumentType) : IDisposable
{
    protected string NodeName { get; } = name;
    protected IArgumentType ArgumentType { get; } = argumentType;
    protected CommandAttribFlags CommandAttrib { get; private set; } = CommandInstance.DefaultCommandAttrib;
    protected CommandNode? Parent { get; private set; }
    protected List<CommandNode> Child { get; } = [];
    protected Func<CommandContext, BaseCommandSource, CancellationToken, Task>? NodeAction { get; private set; }
    protected Func<CommandInformation, BaseCommandSource, bool>? NodeRequire { get; private set; }

    internal void SetAction(Func<CommandContext, BaseCommandSource, CancellationToken, Task> action)
    {
        NodeAction = action;
    }

    internal void SetParent(CommandNode parentNode)
    {
        Parent = parentNode;
    }

    internal void SetCommandAttrib(CommandAttribFlags attr)
    {
        CommandAttrib = attr;
    }

    /// <summary>
    /// Add child node to this node
    /// </summary>
    /// <param name="nextNode"><see cref="SubCommandNode"/> or <see cref="VariableNode"/></param>
    /// <returns>This node self</returns>
    /// <example>
    /// <code>
    /// </code>
    /// </example>
    public CommandNode Then(CommandNode nextNode)
    {
        nextNode.SetParent(this);
        nextNode.SetCommandAttrib(CommandAttrib);
        Child.Add(nextNode);
        return this;
    }

    public CommandNode Require(Func<CommandInformation, BaseCommandSource, bool> predicate)
    {
        NodeRequire = predicate;
        return this;
    }

    /// <summary>
    /// Set node's action
    /// </summary>
    /// <param name="action"><see cref="System.Func{CommandContext, BaseCommandSource, Task}"/></param>
    /// <example>
    /// <code>
    ///     Node.Execute(async (ctx,source) => {
    ///         ...
    ///     });
    /// </code>
    /// </example>
    /// <returns>This node self</returns>
    public CommandNode Execute(Func<CommandContext, BaseCommandSource, CancellationToken, Task> action)
    {
        NodeAction = action;
        return this;
    }

    protected abstract bool IsTokenValid(CommandContext ctx, ref ParsedToken[] tokens,
        [NotNullWhen(false)] out ICommandResult? result);

    public async Task<ICommandResult> ExecuteSelfAsyncAsync(CommandContext ctx, BaseCommandSource source,
        ParsedToken[] tokens)
    {
        if (!IsTokenValid(ctx, ref tokens, out ICommandResult? tempResult)) return tempResult;

        if (NodeAction is not null && (tokens.Length == 0 || Child.Count == 0))
            //无后续token或无子节点 且 定义了节点Action，执行自身
            try
            {
                await NodeAction.TimeoutAfter(TimeSpan.FromSeconds(20))
                    .Invoke(ctx, source);
                return new CommandSuccess();
            }
            catch (Exception ex)
            {
                ctx.Logger.Error(ex, "Node execute failed");
                throw new CommandAbortException($"Node {NodeName} execute failed");
            }

        if (Child.Count == 0)
            //未定义节点Action
            throw new CommandSyntaxException(NodeName);

        //有子节点
        List<ICommandResult> result = [];
        //Ignore Nodes that Not Hits NodeRequire
        foreach (CommandNode node in Child.Where(node =>
                     node.NodeRequire is null || node.NodeRequire(ctx.Information, source)))
        {
            ICommandResult res = await node.ExecuteSelfAsyncAsync(ctx, source, tokens);
            if (res is CommandSuccess)
                //有一个子节点可以执行
                return res;

            result.Add(res);
        }

        //无可执行子节点，对应token异常
        if (tokens.Length == 0)
            return new TooLessArgument(
                result.OfType<TooLessArgument>().SelectMany(item => item.RequiredType).ToList()
            );

        //传递
        List<InvalidArgumentCommandResult> il = result.OfType<InvalidArgumentCommandResult>().ToList();
        return new InvalidArgumentCommandResult(
            il.Count == 0 ? "" : il[0].ErrorToken,
            il.SelectMany(item => item.RequiredType).ToList()
        );
    }

    public abstract string GetArgumentRequire();

    public void Dispose()
    {
        foreach (CommandNode? child in Child) child.Dispose();
        Child.Clear();
        NodeAction = null;
        NodeRequire = null;
        GC.SuppressFinalize(this);
    }
}