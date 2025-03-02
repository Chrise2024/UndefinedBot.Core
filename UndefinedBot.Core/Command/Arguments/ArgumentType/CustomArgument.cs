using UndefinedBot.Core.Command.Arguments.ArgumentRange;
using UndefinedBot.Core.Command.Arguments.TokenContentType;
using UndefinedBot.Core.Command.CommandUtils;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public abstract class CustomArgument : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.Custom;
    public abstract string ArgumentTypeName { get; }
    public abstract IArgumentRange? Range { get; }
    public bool IsValid(ParsedToken token)
    {
        return token is { TokenType: ParsedTokenTypes.Custom, Content: CustomTokenContent content } && ValidContent(content);
    }

    public object GetValue(ParsedToken token) => GetExactTypeValue(token);

    public object GetData(string key, CommandContext ctx)
    {
        if (ctx.ArgumentReference.TryGetValue(key, out ParsedToken token)) return GetExactTypeValue(token);

        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }
    protected abstract bool ValidContent(CustomTokenContent content);
    protected abstract object GetExactTypeValue(ParsedToken token);
}