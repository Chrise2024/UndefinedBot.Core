using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType
{
    public class IntegerArgument(IArgumentRange? range = null) : IArgumentType
    {
        public IArgumentRange? Range { get; } = range;
        public bool IsValid(string token)
        {
            return long.TryParse(token, out long val) && (Range?.InRange(val) ?? true);
        }
        public object GetValue(string token)
        {
            return long.TryParse(token, out long val)
                ? val
                : throw new ArgumentInvalidException($"{token} Is Not Valid Integer");
        }
        public long GetInteger(string token)
        {
            return long.TryParse(token, out long val)
                ? val
                : throw new ArgumentInvalidException($"{token} Is Not Valid Integer");
        }
    }
}