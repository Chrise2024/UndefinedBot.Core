namespace UndefinedBot.Core.Utils;

public interface ILoggerFactory
{
    internal ILogger CreateCategoryLogger<TCategoryName>() where TCategoryName : notnull;
    internal ILogger CreateCategoryLogger<TCategoryName>(string[] tags) where TCategoryName : notnull;
    internal ILogger CreateCategoryLogger(Type type);
    internal ILogger CreateCategoryLogger(Type type, string[] tags);
}