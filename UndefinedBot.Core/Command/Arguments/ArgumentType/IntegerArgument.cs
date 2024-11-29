using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType
{
    public class IntegerArgument(IArgumentRange? range = null) : IArgumentType
    {
        public string TypeName => "Integer";
        public IArgumentRange? Range => range;
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
        public static long GetInteger(string key,CommandContext ctx)
        {
            string token = ctx.ArgumentReference.GetValueOrDefault(key) ??
                           throw new ArgumentInvalidException($"Undefined Argument: {key}");
            return long.TryParse(token, out long val)
                ? val
                : throw new ArgumentInvalidException($"{token} Is Not Valid Integer");
        }
    }
}
