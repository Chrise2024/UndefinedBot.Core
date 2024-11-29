using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments
{
    public class ParsedArgument(IArgumentRange range, object result)
    {
        private IArgumentRange Range => range;
        private object Result => result;
    }
}
