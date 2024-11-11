using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType
{
    public class DateArgument(IArgumentRange? range = null) : IArgumentType
    {
        public IArgumentRange? Range { get; } = range;
        public bool IsValid(string token)
        {
            return DateTime.TryParse(token,out DateTime _);
        }
        public object GetValue(string token)
        {
            return DateTime.TryParse(token, out DateTime val)
                ? val
                : throw new ArgumentInvalidException($"{token} Is Not Valid Positive Integer");
        }
        public DateTime GetDate(string token)
        {
            return DateTime.TryParse(token, out DateTime val)
                ? val
                : throw new ArgumentInvalidException($"{token} Is Not Valid Positive Integer");
        }
    }
}