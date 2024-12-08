namespace UndefinedBot.Core.Command.Arguments.ArgumentRange;

public readonly struct RangeNotIn(IEnumerable<object> range) : IArgumentRange
{
    private object[] Range => range.ToArray();

    public bool InRange(object current)
    {
        return !Range.Contains(current);
    }
    public string GetRangeDescription()
    {
        return $"Not In {{{string.Join(",",Range)}}}";
    }
}