using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments
{
    public class ParsedArgument(IArgumentRange range, object result)
    {
        private IArgumentRange Range { get; } = range;
        private object Result { get; } = result;
    }
}