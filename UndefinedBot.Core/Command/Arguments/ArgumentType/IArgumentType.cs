using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType
{
    public interface IArgumentType
    {
        //public Type AType { get; }
        public string TypeName { get; }
        public IArgumentRange? Range { get; }
        public bool IsValid(string token);
        public object GetValue(string token);
    }
    public class ArgumentInvalidException(string message) : Exception(message);
}
