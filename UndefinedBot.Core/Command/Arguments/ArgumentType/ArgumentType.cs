using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType
{
    public interface IArgumentType
    {
        //public Type AType { get; }
        public IArgumentRange? Range { get; }
        public bool IsValid(string token);
        public object GetValue(string token);
    }
    public class ArgumentInvalidException(string message) : Exception(message);

    public abstract class ArgumentType
    {
        public static readonly Type String = typeof(string);
        public static readonly Type Number= typeof(double);
        public static readonly Type Integer = typeof(Int128);
        public static readonly Type QUin = typeof(QUin);
        public static readonly Type PosInt = typeof(UInt128);
        public static readonly Type Date = typeof(DateTime);
        
    }
}