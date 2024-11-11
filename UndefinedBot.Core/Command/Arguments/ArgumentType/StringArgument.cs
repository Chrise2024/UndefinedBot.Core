using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType
{
    public class StringArgument(IArgumentRange? range = null) : IArgumentType
    {
        public IArgumentRange? Range { get; } = range;
        public bool IsValid(string token)
        {
            return !string.IsNullOrEmpty(token) && (Range?.InRange(token) ?? true);
        }

        public object GetValue(string token)
        {
            return IsValid(token)
                ? token 
                : throw new ArgumentInvalidException("Null String Literal");
        }
        public string GetString(string token)
        {
            return IsValid(token)
                ? token 
                : throw new ArgumentInvalidException("Null String Literal");
        }
    }
}