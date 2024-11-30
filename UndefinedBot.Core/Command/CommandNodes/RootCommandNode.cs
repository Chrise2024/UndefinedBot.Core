using UndefinedBot.Core.Command.Arguments.ArgumentType;

namespace UndefinedBot.Core.Command.CommandNodes
{
    /// <summary>
    /// Root node of command tree.Only use in <see cref="UndefinedBot.Core.Command.CommandInstance"/>.
    /// </summary>
    /// <param name="name">Node name,will be same as command name</param>
    internal class RootCommandNode(string name) : ICommandNode
    {
        public string NodeName => name;
        public IArgumentType ArgumentType => new StringArgument();
        public ICommandNode? Parent { get; private set; }
        public List<ICommandNode> Child { get; private set; } = [];
        public CommandNodeAction? NodeAction { get; private set; }
        /// <summary>
        /// <para>Set action of the node</para>
        /// <para>Only use in api internal</para>
        /// </summary>
        public void SetAction(CommandNodeAction action)
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
        public ICommandNode Execute(CommandNodeAction action)
        {
            NodeAction = action;
            return this;
        }
        public async Task<ExecuteStatus> ExecuteSelf(CommandContext ctx,List<string> tokens)
        {
            if (NodeAction != null && (tokens.Count == 0 || Child.Count == 0))
            {
                //无后续token或无子节点 且 定义了节点Action，执行自身
                try
                {
                    await NodeAction(ctx);
                    return ExecuteStatus.Success;
                }
                catch(Exception ex)
                {
                    ctx.Logger.Error(ex.Message);
                    ctx.Logger.Error(ex.StackTrace ?? "");
                    throw new CommandAbortException($"Node {NodeName} execute failed");
                }
            }
            else
            {
                if (Child.Count > 0)
                {
                    //有子节点
                    if (tokens.Count == 0)
                    {
                        //缺token执行子节点
                        throw new TooLessArgumentException(Child.Select(item =>
                            item is SubCommandNode ? $"<{item.NodeName}>" : $"[{item.ArgumentType.TypeName}]"
                        ));
                    }
                    //有足够的token执行子节点
                    foreach (ICommandNode node in Child)
                    {
                        if (await node.ExecuteSelf(ctx, tokens) == ExecuteStatus.Success)
                        {
                            //有一个子节点可以执行
                            return ExecuteStatus.Success;
                        }
                    }
                    //无可执行子节点，对应token异常
                    throw new InvalidArgumentException(tokens[0],Child.Select(item =>
                        item is SubCommandNode ? $"<{item.NodeName}>" : $"[{item.ArgumentType.TypeName}]"
                    ));
                }
                else
                {
                    //未定义节点Action
                    throw new CommandSyntaxException(NodeName);
                }
            }
        }
    }
}
