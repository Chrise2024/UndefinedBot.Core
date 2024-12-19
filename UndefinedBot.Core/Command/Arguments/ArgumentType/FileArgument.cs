using System.Text.Json;
using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public class FileArgument : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.File;
    public string ArgumentTypeName => "File";
    public IArgumentRange? Range => null;
    public bool IsValid(ParsedToken token)
    {
        return token.TokenType == ParsedTokenTypes.File;
    }
    public object GetValue(ParsedToken token) => GetExactTypeValue(token);

    public static FileContent GetImage(string key,CommandContext ctx)
    {
        if (ctx.ArgumentReference.TryGetValue(key, out ParsedToken? token))
        {
            return GetExactTypeValue(token);
        }
        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }
    private static FileContent GetExactTypeValue(ParsedToken token)
    {
        return token.TokenType == ParsedTokenTypes.File ? JsonSerializer.Deserialize<FileContent>(token.SerializedContent)! : throw new ArgumentInvalidException("Token Is Not File");
    }
}

public class FileContent(string fileUrl,string fileUnique,int size)
{
    public string FileUrl => fileUrl;
    public string FileUnique => fileUnique;
    public int Size => size;
}
