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

public class ArgumentInvalidException(string message) : Exception(message);

public enum ArgumentTypes
{
    String = 0,
    Integer = 1,
    PositiveInteger = 2,
    Number = 3,
    Date = 4,
    User = 5,
    Reply = 6,
    Image = 7,
    File = 8,
}