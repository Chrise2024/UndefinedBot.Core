using UndefinedBot.Core.Command.Arguments.ArgumentType;

namespace UndefinedBot.Core.Command
{
    public delegate Task CommandNodeAction(CommandContext ctx);

    public delegate ICommandNode RegisterNodeAction(ICommandNode parentNode);
    public enum CommandNodeType
    {
        RootCommand = 0,
        SubCommand = 1,
        Variable = 2,
    }
    public interface ICommandNode
    {
        public string NodeName { get; }
        public CommandNodeType NodeType { get; }
        public IArgumentType ArgumentType { get; }
        public ICommandNode? Parent { get; }
        public List<ICommandNode> Child { get; }
        public CommandNodeAction? NodeAction { get; }
        internal void SetParent(ICommandNode parentNode);
        internal void SetAction(CommandNodeAction action);
        public ICommandNode Then(ICommandNode nextNode);
        public ICommandNode Execute(CommandNodeAction action);
        internal Task ExecuteSelf(CommandContext ctx,List<string> tokens);
    }
    /// <summary>
    /// Root node of command tree.Only use in <see cref="UndefinedBot.Core.Command.CommandInstance"/>.
    /// </summary>
    /// <param name="name">Node name,will be same as command name</param>
    internal class RootCommandNode(string name) : ICommandNode
    {
        public string NodeName { get; } = name;
        public CommandNodeType NodeType { get; } = CommandNodeType.RootCommand;
        public IArgumentType ArgumentType { get; } = new StringArgument();
        public ICommandNode? Parent { get; private set; }
        public List<ICommandNode> Child { get; } = [];
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
        /// <param name="nextNode"><see cref="UndefinedBot.Core.Command.SubCommandNode"/> or <see cref="UndefinedBot.Core.Command.VariableNode"/></param>
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
        /// <param name="action"><see cref="UndefinedBot.Core.Command.CommandNodeAction"/></param>
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
        public async Task ExecuteSelf(CommandContext ctx,List<string> tokens)
        {
            foreach (ICommandNode child in Child)
            {
                await child.ExecuteSelf(ctx,tokens);
            }
            if (NodeAction != null)
            {
                await NodeAction(ctx);
                throw new CommandFinishException("Completed");
            }
        }
    }
    public class SubCommandNode(string name):ICommandNode
    {
        public string NodeName { get; } = name;
        public CommandNodeType NodeType { get; } = CommandNodeType.SubCommand;
        public IArgumentType ArgumentType { get; } = new StringArgument();
        public ICommandNode? Parent { get; private set; }
        public List<ICommandNode> Child { get; } = [];
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
        /// <param name="nextNode"><see cref="UndefinedBot.Core.Command.SubCommandNode"/> or <see cref="UndefinedBot.Core.Command.VariableNode"/></param>
        /// <returns>This node self</returns>
        /// <example>
        /// <code>
        ///     Node.Then(new SubCommandNode("foo",new StringArgument()))
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
        /// <param name="action"><see cref="UndefinedBot.Core.Command.CommandNodeAction"/></param>
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
        public async Task ExecuteSelf(CommandContext ctx,List<string> tokens)
        {
            if (tokens.Count > 0)
            {
                string current = tokens[0];
                if (current.Equals(NodeName))
                {
                    foreach (ICommandNode child in Child)
                    {
                        await child.ExecuteSelf(ctx,tokens[1..]);
                    }
                    if (NodeAction != null)
                    {
                        await NodeAction(ctx);
                        throw new CommandFinishException("Completed");
                    }
                }
            }
        }
    }
    public class VariableNode(string name,IArgumentType argumentType):ICommandNode
    {
        public string NodeName { get; } = name;
        public CommandNodeType NodeType { get; } = CommandNodeType.Variable;
        public IArgumentType ArgumentType { get; } = argumentType;
        public ICommandNode? Parent { get; private set; }
        public List<ICommandNode> Child { get; } = [];
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
        /// <param name="nextNode"><see cref="UndefinedBot.Core.Command.SubCommandNode"/> or <see cref="UndefinedBot.Core.Command.VariableNode"/></param>
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
        /// <param name="action"><see cref="UndefinedBot.Core.Command.CommandNodeAction"/></param>
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
        public async Task ExecuteSelf(CommandContext ctx,List<string> tokens)
        {
            if (tokens.Count > 0)
            {
                string current = tokens[0];
                if (ArgumentType.IsValid(current))
                {
                    ctx.ArgumentReference[NodeName] = current;
                    foreach (ICommandNode child in Child)
                    {
                        await child.ExecuteSelf(ctx,tokens[1..]);
                    }
                    if (NodeAction != null)
                    {
                        await NodeAction(ctx);
                        throw new CommandFinishException("Completed");
                    }
                }
            }
        }
    }

    public class CommandFinishException(string message) : Exception(message);
}
