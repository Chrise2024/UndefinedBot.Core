namespace UndefinedBot.Core.Command.Arguments.ArgumentRange
{
    public interface IArgumentRange
    {
        public bool InRange(object current);
        public string GetRangeDescription();
    }
}
