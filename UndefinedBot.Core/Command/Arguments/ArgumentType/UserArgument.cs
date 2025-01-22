using UndefinedBot.Core.Command.Arguments.ArgumentRange;
using UndefinedBot.Core.Command.Arguments.TokenContentType;

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

    public object GetValue(ParsedToken token) => GetExactTypeValue(token);

    public static UserContent GetUser(string key, CommandContext ctx)
    {
        if (ctx.ArgumentReference.TryGetValue(key, out ParsedToken token))
        {
            return GetExactTypeValue(token);
        }

        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }

    /// <summary>
    /// 'User' may be a string of number(id) ?
    /// </summary>
    private static UserContent GetExactTypeValue(ParsedToken token)
    {
        return token is { TokenType: ParsedTokenTypes.User, Content: UserContent user }
            ? user
            : throw new ArgumentInvalidException("Token Is Not User");
    }
}