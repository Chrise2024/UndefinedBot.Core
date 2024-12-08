namespace UndefinedBot.Core.Command.Arguments.ArgumentRange;

public readonly struct RangeIn(IEnumerable<object> range) : IArgumentRange
{
    private object[] Range => range.ToArray();

    public bool InRange(object current)
    {
        return Range.Contains(current);
    }

    public string GetRangeDescription()
    {
        return $"In {{{string.Join(",",Range)}}}";
    }
}