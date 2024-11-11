using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType
{
    public class PosIntArgument(IArgumentRange? range = null) : IArgumentType
    {
        public IArgumentRange? Range { get; } = range;
        public bool IsValid(string token)
        {
            return ulong.TryParse(token, out ulong val) && (Range?.InRange(val) ?? true);
        }
        public object GetValue(string token)
        {
            return ulong.TryParse(token, out ulong val)
                ? val
                : throw new ArgumentInvalidException($"{token} Is Not Valid Positive Integer");
        }
        public ulong GetPosInt(string token)
        {
            return ulong.TryParse(token, out ulong val)
                ? val
                : throw new ArgumentInvalidException($"{token} Is Not Valid Positive Integer");
        }
    }
}