namespace UndefinedBot.Core.Command.Arguments.ArgumentRange
{
    public class RangeIn(IEnumerable<object> range) : IArgumentRange
    {
        private IEnumerable<object> Range { get; } = range;

        public bool InRange(object current)
        {
            return Range.Contains(current);
        }
    }
}