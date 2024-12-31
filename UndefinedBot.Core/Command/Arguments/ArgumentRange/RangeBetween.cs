namespace UndefinedBot.Core.Command.Arguments.ArgumentRange;

public sealed class RangeBetween<T> : IArgumentRange where T : IComparable
{
    public T Maximum { get; }
    public T Minimum { get; }
    public RangeBetween(T range1, T range2)
    {
        if (range1.CompareTo(range2) > 0)
        {
            Maximum = range1;
            Minimum = range2;
        }
        else
        {
            Maximum = range2;
            Minimum = range1;
        }
    }
    public bool InRange(object current)
    {
        try
        {
            return Maximum.CompareTo((T)current) > 0 && Minimum.CompareTo((T)current) < 0;
        }
        catch
        {
            return false;
        }
    }

    public string GetRangeDescription()
    {
        return $"{Minimum} ~ {Maximum}";
    }
}
