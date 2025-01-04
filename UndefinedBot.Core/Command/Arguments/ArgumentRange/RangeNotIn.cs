using System.Text.Json;

namespace UndefinedBot.Core.Command.Arguments.ArgumentRange;

public sealed class RangeNotIn<T> : IArgumentRange where T : IEquatable<T>
{
    public List<T> Range { get; }
    public string DescriptionString { get; }

    public RangeNotIn(List<T> range)
    {
        Range = range.ToList();
        DescriptionString = string.Join(",", Range.Select(item => JsonSerializer.Serialize(item)));
    }

    public bool InRange(object current)
    {
        return current is not T tc || !Range.Any(item => item.Equals(tc));
    }

    public string GetRangeDescription()
    {
        return $"Not In {{{DescriptionString}}}";
    }
}