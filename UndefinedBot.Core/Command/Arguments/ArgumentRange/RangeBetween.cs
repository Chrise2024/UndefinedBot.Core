using System;

namespace UndefinedBot.Core.Command.Arguments.ArgumentRange
{
    public class RangeBetween : IArgumentRange
    {
        public object Maximum { get; }
        public object Minimum { get; }
        public RangeBetween(object range1, object range2)
        {
            int? res = ((IComparable)range1)?.CompareTo(range2);
            if (res != null && range1 != null && range2 != null)
            {
                if (res > 0)
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
            else
            {
                Maximum = 0;
                Minimum = 0;
                throw new ArgumentOutOfRangeException();
            }
        }
        public bool InRange(object current)
        {
            try
            {
                return ((IComparable)Maximum)?.CompareTo(current) > 0 && ((IComparable)Minimum).CompareTo(current) < 0;
            }
            catch
            {
                return false;
            }
        }
    }
}