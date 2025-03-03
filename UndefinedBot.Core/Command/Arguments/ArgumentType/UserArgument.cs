using UndefinedBot.Core.Command.Arguments.ArgumentRange;
using UndefinedBot.Core.Command.Arguments.TokenContentType;
using UndefinedBot.Core.Command.CommandUtils;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public sealed class UserArgument(IArgumentRange? range = null) : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.User;
    public string ArgumentTypeName => "At";
    public IArgumentRange? Range => range;

    public bool IsValid(ParsedToken token)
    {
        return token.TokenType == ParsedTokenTypes.User;
    }

    public static UserTokenContent GetUser(string key, CommandContext ctx)
    {
        return ctx.GetArgumentReference(key) is { TokenType: ParsedTokenTypes.User, Content: UserTokenContent user }
            ? user
            : throw new ArgumentInvalidException("Token Is Not User");
    }
}