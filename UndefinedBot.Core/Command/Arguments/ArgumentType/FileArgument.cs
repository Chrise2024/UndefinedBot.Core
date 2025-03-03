using UndefinedBot.Core.Command.Arguments.ArgumentRange;
using UndefinedBot.Core.Command.Arguments.TokenContentType;
using UndefinedBot.Core.Command.CommandUtils;

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

    public static FileTokenContent GetFile(string key, CommandContext ctx)
    {
        return ctx.GetArgumentReference(key) is { TokenType: ParsedTokenTypes.File, Content: FileTokenContent file }
            ? file
            : throw new ArgumentInvalidException("Token Is Not File");
    }
}