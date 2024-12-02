namespace UndefinedBot.Core.Command.Arguments.ArgumentRange
{
    public class RangeNotIn(IEnumerable<object> range) : IArgumentRange
    {
        private IEnumerable<object> Range => range;

        public bool InRange(object current)
        {
            return !Range.Contains(current);
        }
        public string GetRangeDescription()
        {
            return $"Not In {{{string.Join(",",Range)}}}";
        }
    }
}
