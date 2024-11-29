using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType
{
    public class PosIntArgument(IArgumentRange? range = null) : IArgumentType
    {
        public string TypeName => "Positive Integer";
        public IArgumentRange? Range => range;
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
        public static ulong GetPosInt(string key,CommandContext ctx)
        {
            string token = ctx.ArgumentReference.GetValueOrDefault(key) ??
                           throw new ArgumentInvalidException($"Undefined Argument: {key}");
            return ulong.TryParse(token, out ulong val)
                ? val
                : throw new ArgumentInvalidException($"{token} Is Not Valid Positive Integer");
        }
    }
}
