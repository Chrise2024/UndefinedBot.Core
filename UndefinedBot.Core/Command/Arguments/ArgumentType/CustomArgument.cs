using UndefinedBot.Core.Command.Arguments.ArgumentRange;
using UndefinedBot.Core.Command.Arguments.TokenContentType;
using UndefinedBot.Core.Command.CommandUtils;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

/// <summary>
/// To override the custom argument, you need to inherit this class and implement the abstract methods
/// <remarks>Suggest to provide static method to get the value in command logic, like other types</remarks>
/// </summary>
public abstract class CustomArgument : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.Custom;
    public abstract string ArgumentTypeName { get; }
    public abstract IArgumentRange? Range { get; }

    public bool IsValid(ParsedToken token)
    {
        return token is { TokenType: ParsedTokenTypes.Custom, Content: CustomTokenContent content } &&
               ValidContent(content);
    }

    protected abstract bool ValidContent(CustomTokenContent content);
}