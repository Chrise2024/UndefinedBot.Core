using System.Diagnostics.CodeAnalysis;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Command.CommandResult;
using UndefinedBot.Core.Command.CommandUtils;

namespace UndefinedBot.Core.Command.CommandNode;

/// <summary>
/// Root node of command tree.Only use in <see cref="UndefinedBot.Core.Command.CommandInstance"/>.
/// </summary>
/// <param name="name">Node name,will be same as command name</param>
internal sealed class RootCommandNode(string name) : CommandNode(name, new StringArgument())
{
    internal override bool IsTokenValid(CommandContext ctx, ref ParsedToken[] tokens,
        [NotNullWhen(false)] out ICommandResult? result)
    {
        result = null;
        return true;
    }

    public override string GetArgumentRequire()
    {
        return $"<{NodeName}>";
    }
}