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

    public object GetValue(ParsedToken token)
    {
        return GetExactTypeValue(token);
    }

    public static UserTokenContent GetUser(string key, CommandContext ctx)
    {
        if (ctx.ArgumentReference.TryGetValue(key, out ParsedToken token)) return GetExactTypeValue(token);

        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }

    /// <summary>
    /// 'User' may be a string of number(id) ?
    /// </summary>
    private static UserTokenContent GetExactTypeValue(ParsedToken token)
    {
        return token is { TokenType: ParsedTokenTypes.User, Content: UserTokenContent user }
            ? user
            : throw new ArgumentInvalidException("Token Is Not User");
    }
}