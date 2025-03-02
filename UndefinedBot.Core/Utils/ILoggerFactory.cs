namespace UndefinedBot.Core.Utils;

internal interface ILoggerFactory
{
    public ILogger CreateCategoryLogger<TCategoryName>() where TCategoryName : notnull;
    public ILogger CreateCategoryLogger<TCategoryName>(string[] tags) where TCategoryName : notnull;
    public ILogger CreateCategoryLogger(Type type);
    public ILogger CreateCategoryLogger(Type type, string[] tags);
}