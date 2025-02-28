using System.Diagnostics.CodeAnalysis;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Command.Arguments.TokenContentType;
using UndefinedBot.Core.Command.CommandResult;
using UndefinedBot.Core.Command.CommandUtils;

namespace UndefinedBot.Core.Command.CommandNode;

public sealed class SubCommandNode(string name) : CommandNode(name, new StringArgument())
{
    internal override bool IsTokenValid(CommandContext ctx,ref ParsedToken[] tokens, [NotNullWhen(false)] out ICommandResult? result)
    {
        if (tokens.Length == 0)
        {
            result = new TooLessArgument([$"[{GetArgumentRequire()}]"]);
            return false;
        }

        // if (tokens[0].TokenType != ParsedTokenTypes.Normal || (tokens[0].Content is TextContent text &&
        //                                                        text.Text != NodeName))
        if (tokens[0] is not {TokenType:ParsedTokenTypes.Text, Content:TextTokenContent text} || text.Text != NodeName)
        {
            result = new InvalidArgumentCommandResult(tokens[0].TokenType.ToString(), [GetArgumentRequire()]);
            return false;
        }
        
        tokens = tokens[1..];
        result = null;
        return true;
    }

    public override string GetArgumentRequire()
    {
        return $"<{NodeName}>";
    }
}