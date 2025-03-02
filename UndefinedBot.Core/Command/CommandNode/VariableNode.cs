using System.Diagnostics.CodeAnalysis;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Command.CommandResult;
using UndefinedBot.Core.Command.CommandUtils;

namespace UndefinedBot.Core.Command.CommandNode;

public sealed class VariableNode(string name, IArgumentType argumentType) : CommandNode(name, argumentType)
{
    protected override bool IsTokenValid(CommandContext ctx, ref ParsedToken[] tokens,
        [NotNullWhen(false)] out ICommandResult? result)
    {
        if (tokens.Length == 0)
        {
            result = new TooLessArgument([$"[{GetArgumentRequire()}]"]);
            return false;
        }

        if (!ArgumentType.IsValid(tokens[0]))
        {
            result = new InvalidArgumentCommandResult(tokens[0].TokenType.ToString(), [GetArgumentRequire()]);
            return false;
        }

        ctx.ArgumentReference[NodeName] = tokens[0];

        tokens = tokens[1..];
        result = null;
        return true;
    }

    public override string GetArgumentRequire()
    {
        return ArgumentType.Range is null
            ? $"[{ArgumentType.ArgumentTypeName}]"
            : $"[{ArgumentType.ArgumentTypeName} ({ArgumentType.Range.GetRangeDescription()})]";
    }
}