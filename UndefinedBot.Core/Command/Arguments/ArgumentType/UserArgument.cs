using System.Text.RegularExpressions;
using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public class UserArgument(IArgumentRange? range = null) : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.User;
    public string ArgumentTypeName => "At";
    public IArgumentRange? Range => range;
    public bool IsValid(ParsedToken token)
    {
        return token.TokenType == ParsedTokenTypes.User;
    }
    public object GetValue(ParsedToken token) => GetExactTypeValue(token);
    public static string GetUser(string key,CommandContext ctx)
    {
        if (ctx.ArgumentReference.TryGetValue(key, out ParsedToken? token))
        {
            return GetExactTypeValue(token);
        }
        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }
    private static string GetExactTypeValue(ParsedToken token)
    {
        return token.TokenType == ParsedTokenTypes.User ? token.SerializedContent : throw new ArgumentInvalidException("Token Is Not User");
    }
}
