using System.Text.Json;

namespace UndefinedBot.Core.Command.Arguments.ArgumentRange;

public sealed class RangeIn<T> : IArgumentRange where T : IEquatable<T>
{
    public readonly T[] Range;
    public readonly string DescriptionString;

    public RangeIn(IEnumerable<T> range)
    {
        Range = range.ToArray();
        DescriptionString = string.Join(",", Range.Select(item => JsonSerializer.Serialize(item)));
    }

    public bool InRange(object current)
    {
        return current is T tc && Array.Find(Range, item => item.Equals(tc)) != null;
    }

    public string GetRangeDescription()
    {
        return $"In {{{DescriptionString}}}";
    }
}