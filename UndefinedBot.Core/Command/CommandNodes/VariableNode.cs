using Newtonsoft.Json;
using UndefinedBot.Core.Command.Arguments.ArgumentType;

namespace UndefinedBot.Core.Command.CommandNodes
{
    public class VariableNode(string name,IArgumentType argumentType):ICommandNode
    {
        public string NodeName => name;
        public IArgumentType ArgumentType => argumentType;
        public ICommandNode? Parent { get; private set; }
        public List<ICommandNode> Child  { get; private set; } = [];
        public CommandNodeAction? NodeAction { get; private set; }
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
            if (tokens.Count >= 1)
            {
                string current = tokens[0];
                if (ArgumentType.IsValid(current))
                {
                    ctx.ArgumentReference[NodeName] = current;
                    //无子节点，执行自身
                    if (Child.Count == 0)
                    {
                        if (NodeAction != null)
                        {
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
                            throw new CommandSyntaxException(NodeName);
                        }
                    }
                    else
                    {
                        if (tokens.Count >= 2)
                        {
                            //有足够的token执行子节点
                            foreach (ICommandNode node in Child)
                            {
                                if (await node.ExecuteSelf(ctx, tokens[1..]) == ExecuteStatus.Success)
                                {
                                    //有一个子节点可以执行
                                    return ExecuteStatus.Success;
                                }
                            }
                            //无可执行子节点，对应token异常
                            throw new InvalidArgumentException(tokens[1],JsonConvert.SerializeObject(Child.Select(item => item is SubCommandNode ? item.NodeName : item.ArgumentType.TypeName)));
                        }
                        else
                        {
                            //有子节点但是token不足
                            throw new TooLessArgumentException();
                        }
                    }
                }
                else
                {
                    //正常遍历
                    return ExecuteStatus.InvalidArgument;
                }
            }
            else
            {
                //正常遍历
                return ExecuteStatus.NullArgument;
            }
        }
    }
}
