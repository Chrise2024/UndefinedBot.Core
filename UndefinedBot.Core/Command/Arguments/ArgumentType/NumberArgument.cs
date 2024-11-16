using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType
{
    public class NumberArgument(IArgumentRange? range = null) : IArgumentType
    {
        public IArgumentRange? Range { get; } = range;
        public bool IsValid(string token)
        {
            return double.TryParse(token, out double val) && (Range?.InRange(val) ?? true);
        }

        public object GetValue(string token)
        {
            return double.TryParse(token, out double val)
                ? val
                : throw new ArgumentInvalidException($"{token} Is Not Valid Number");
        }
        public static double GetNumber(string key,CommandContext ctx)
        {
            string token = ctx.ArgumentReference.GetValueOrDefault(key) ??
                           throw new ArgumentInvalidException($"Undefined Argument: {key}");
            return double.TryParse(token, out double val)
                ? val
                : throw new ArgumentInvalidException($"{token} Is Not Valid Number");
        }
    }
}