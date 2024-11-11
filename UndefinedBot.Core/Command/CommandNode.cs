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
        public void SetParent(ICommandNode parentNode);
        public void SetAction(CommandNodeAction action);
        public ICommandNode Then(ICommandNode nextNode);
        public ICommandNode Execute(CommandNodeAction action);
        public Task ExecuteSelf(CommandContext ctx);
    }
    public class RootCommandNode(string name, IArgumentType argumentType) : ICommandNode
    {
        public string NodeName { get; } = name;
        public CommandNodeType NodeType { get; } = CommandNodeType.RootCommand;
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
        public ICommandNode Then(ICommandNode nextNode)
        {
            nextNode.SetParent(this);
            Child.Add(nextNode);
            return this;
        }
        public ICommandNode Execute(CommandNodeAction action)
        {
            NodeAction = action;
            return this;
        }
        public async Task ExecuteSelf(CommandContext ctx)
        {
            foreach (ICommandNode child in Child)
            {
                await child.ExecuteSelf(ctx);
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
        public ICommandNode Then(ICommandNode nextNode)
        {
            nextNode.SetParent(this);
            Child.Add(nextNode);
            return this;
        }
        public ICommandNode Execute(CommandNodeAction action)
        {
            NodeAction = action;
            return this;
        }
        public async Task ExecuteSelf(CommandContext ctx)
        {
            if (ctx.ArgumentReference.TryGetValue(NodeName, out string? current) && current.Equals(NodeName))
            {
                foreach (ICommandNode child in Child)
                {
                    await child.ExecuteSelf(ctx);
                }
                if (NodeAction != null)
                {
                    await NodeAction(ctx);
                    throw new CommandFinishException("Completed");
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
        public ICommandNode Then(ICommandNode nextNode)
        {
            nextNode.SetParent(this);
            Child.Add(nextNode);
            return this;
        }
        public ICommandNode Execute(CommandNodeAction action)
        {
            NodeAction = action;
            return this;
        }
        public async Task ExecuteSelf(CommandContext ctx)
        {
            if (ctx.ArgumentReference.TryGetValue(NodeName, out string? current) && ArgumentType.IsValid(current))
            {
                foreach (ICommandNode child in Child)
                {
                    await child.ExecuteSelf(ctx);
                }
                if (NodeAction != null)
                {
                    await NodeAction(ctx);
                    throw new CommandFinishException("Completed");
                }
            } 
        }
    }

    public class CommandFinishException(string message) : Exception(message);
}