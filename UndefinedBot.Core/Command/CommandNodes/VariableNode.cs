using UndefinedBot.Core.Command.CommandResult;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.Arguments.ArgumentType;

namespace UndefinedBot.Core.Command.CommandNodes;

public class VariableNode(string name,IArgumentType argumentType):ICommandNode
{
    public string NodeName => name;
    public IArgumentType ArgumentType => argumentType;
    public ICommandNode? Parent { get; private set; }
    public List<ICommandNode> Child  { get; private set; } = [];
    public Func<CommandContext,Task>? NodeAction { get; private set; }
    public void SetAction(Func<CommandContext,Task> action)
    {
        NodeAction = action;
    }
    public void SetParent(ICommandNode parentNode)
    {
        Parent = parentNode;
    }
    /// <summary>
    /// Add child node to this node
    /// </summary>
    /// <param name="nextNode"><see cref="UndefinedBot.Core.Command.CommandNodes.SubCommandNode"/> or <see cref="UndefinedBot.Core.Command.CommandNodes.VariableNode"/></param>
    /// <returns>This node self</returns>
    /// <example>
    /// <code>
    /// </code>
    /// </example>
    public ICommandNode Then(ICommandNode nextNode)
    {
        nextNode.SetParent(this);
        Child.Add(nextNode);
        return this;
    }
    /// <summary>
    /// Set node's action
    /// </summary>
    /// <param name="action"><see cref="UndefinedBot.Core.Command.CommandNodes.CommandNodeAction"/></param>
    /// <example>
    /// <code>
    ///     Node.Execute(async (ctx) => {
    ///         ...
    ///     });
    /// </code>
    /// </example>
    /// <returns>This node self</returns>
    public ICommandNode Execute(Func<CommandContext,Task> action)
    {
        NodeAction = action;
        return this;
    }
    public async Task<ICommandResult> ExecuteSelf(CommandContext ctx,List<ParsedToken> tokens)
    {
        if (tokens.Count == 0)
        {
            return new TooLessArgument([$"[{GetArgumentRequire()}]"]);
        }

        if (!ArgumentType.IsValid(tokens[0]))
        {
            return new InvalidArgument(tokens[0].Content, [GetArgumentRequire()]);
        }

        ctx._argumentReference[NodeName] = tokens[0];
        if (NodeAction != null && (tokens.Count == 1 || Child.Count == 0))
        {
            //无后续token或无子节点 且 定义了节点Action，执行自身
            try
            {
                await Task.WhenAny(NodeAction(ctx),Task.Delay(TimeSpan.FromSeconds(20)));
                return new CommandSuccess();
            }
            catch (Exception ex)
            {
                ctx.Logger.Error(ex.Message);
                ctx.Logger.Error(ex.StackTrace ?? "");
                throw new CommandAbortException($"Node {NodeName} execute failed");
            }
        }

        if (Child.Count == 0)
        {
            //未定义节点Action
            throw new CommandSyntaxException(NodeName);
        }

        //有子节点
        List<ICommandResult> result = [];
        foreach (ICommandNode node in Child)
        {
            ICommandResult res = await node.ExecuteSelf(ctx, tokens[1..]);
            if (res is CommandSuccess)
            {
                //有一个子节点可以执行
                return new CommandSuccess();
            }

            result.Add(res);
        }

        //无可执行子节点，对应token异常
        if (tokens.Count == 1)
        {
            return new TooLessArgument(
                result.OfType<TooLessArgument>().SelectMany(item => item.RequiredType).ToList()
            );
        }

        //传递
        List<InvalidArgument> il = result.OfType<InvalidArgument>().ToList();
        return new InvalidArgument(
            il.Count == 0 ? "" : il[0].ErrorToken,
            il.SelectMany(item => item.RequiredType).ToList()
        );
    }
    public string GetArgumentRequire()
    {
        return ArgumentType.Range == null ? $"[{ArgumentType.ArgumentTypeName}]" : $"[{ArgumentType} ({ArgumentType.Range.GetRangeDescription()})]";
    }
}
