namespace UndefinedBot.Core.Command.Arguments.ArgumentRange
{
    public class RangeBetween<T> : IArgumentRange where T : IComparable
    {
        private T _maximum { get; }
        private T _minimum { get; }
        public RangeBetween(T range1, T range2)
        {
            if (range1.CompareTo(range2) > 0)
            {
                _maximum = range1;
                _minimum = range2;
            }
            else
            {
                _maximum = range2;
                _minimum = range1;
            }
        }
        public bool InRange(object current)
        {
            try
            {
                return _maximum.CompareTo((T)current) > 0 && _minimum.CompareTo((T)current) < 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
