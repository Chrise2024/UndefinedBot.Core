using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public interface IArgumentType
{
    public ArgumentTypes ArgumentType { get; }
    public string ArgumentTypeName { get; }
    public IArgumentRange? Range { get; }
    public bool IsValid(ParsedToken token);
    public object GetValue(ParsedToken token);
}