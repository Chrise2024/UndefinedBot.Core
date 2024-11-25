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
        public static string GetString(string key,CommandContext ctx)
        {
            string token = ctx.ArgumentReference.GetValueOrDefault(key) ??
                           throw new ArgumentInvalidException($"Undefined Argument: {key}");
            return string.IsNullOrEmpty(token)
                ? throw new ArgumentInvalidException("Null String Literal")
                : token;
        }
    }
}
