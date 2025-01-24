using UndefinedBot.Core.Command.Arguments.ArgumentRange;
using UndefinedBot.Core.Command.Arguments.TokenContentType;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public sealed class FileArgument : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.File;
    public string ArgumentTypeName => "File";
    public IArgumentRange? Range => null;

    public bool IsValid(ParsedToken token)
    {
        return token.TokenType == ParsedTokenTypes.File;
    }
    public object GetValue(ParsedToken token) => GetExactTypeValue(token);

    public static FileTokenContent GetFile(string key, CommandContext ctx)
    {
        if (ctx.ArgumentReference.TryGetValue(key, out ParsedToken token))
        {
            return GetExactTypeValue(token);
        }

        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }
    private static FileTokenContent GetExactTypeValue(ParsedToken token)
    {
        return token is { TokenType: ParsedTokenTypes.File, Content: FileTokenContent file }
            ? file
            : throw new ArgumentInvalidException("Token Is Not File");
    }
}