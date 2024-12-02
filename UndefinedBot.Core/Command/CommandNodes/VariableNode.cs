using UndefinedBot.Core.Command.CommandResult;
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
        public async Task<ICommandResult> ExecuteSelf(CommandContext ctx,List<string> tokens)
        {
            if (tokens.Count >= 1)
            {
                if (ArgumentType.IsValid(tokens[0]))
                {
                    ctx.ArgumentReference[NodeName] = tokens[0];
                    if (NodeAction != null && (tokens.Count == 1 || Child.Count == 0))
                    {
                        //无后续token或无子节点 且 定义了节点Action，执行自身
                        try
                        {
                            await NodeAction(ctx);
                            return new CommandSuccess();
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
                                List<string> r = [];
                                foreach (ICommandResult index in result)
                                {
                                    r.AddRange((index as TooLessArgument)?.RequiredType ?? []);
                                }
                                return new TooLessArgument(r);
                            }
                            else
                            {
                                List<string> r = [];
                                List<InvalidArgument?> il = result.Select(item => item as InvalidArgument).ToList();
                                foreach (InvalidArgument? index in il)
                                {
                                    r.AddRange(index?.RequiredType ?? []);
                                }
                                //传递
                                return new InvalidArgument(il.FirstOrDefault(item => item?.ErrorToken != null)?.ErrorToken ?? "",r);
                            }
                        }
                        else
                        {
                            //未定义节点Action
                            return new InvalidSyntax(NodeName);
                        }
                    }
                }
                else
                {
                    //正常遍历
                    return new InvalidArgument(tokens[0],[GetArgumentRequire()]);
                }
            }
            else
            {
                //正常遍历
                return new TooLessArgument([$"[{GetArgumentRequire()}]"]);
            }
        }
        public string GetArgumentRequire()
        {
            return ArgumentType.Range == null ? $"[{ArgumentType.TypeName}]" : $"[{ArgumentType} ({ArgumentType.Range.GetRangeDescription()})]";
        }
    }
}
