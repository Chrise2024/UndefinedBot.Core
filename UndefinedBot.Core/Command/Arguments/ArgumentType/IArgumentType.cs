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
    Date = 0,
    Image = 1,
    Integer = 2,
    Number = 3,
    PosInt = 4,
    Reply = 5,
    String = 6,
    Uin = 7,
}