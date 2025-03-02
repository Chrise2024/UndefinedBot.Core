using Microsoft.Extensions.Logging;
using MsILogger = Microsoft.Extensions.Logging.ILogger;
using InternalILogger = UndefinedBot.Core.Utils.ILogger;

namespace UndefinedBot.Net.Utils.Logging;

public sealed class ExtendableLogger : BaseLogger
{
    protected override string Template { get; }
    protected override string[] Tags { get; } = [];
    protected override MsILogger RootLogger { get; }
    private ILoggerFactory LoggerFactory { get; }
    private Type CategoryType { get; }

    private ExtendableLogger(ILoggerFactory loggerFactory, string[] subSpace, Type categoryType)
    {
        LoggerFactory = loggerFactory;
        RootLogger = loggerFactory.CreateLogger(categoryType);
        Tags = subSpace;
        CategoryType = categoryType;
        string extendTemplate = "";
        for (int i = 0; i < Tags.Length; i++) extendTemplate += $"[{{Tag{i}}}] ";

        Template = "[{Time}] [{LogLevel}] " + extendTemplate + "{Message}";
    }

    private ExtendableLogger(ILoggerFactory loggerFactory, Type categoryType)
    {
        LoggerFactory = loggerFactory;
        RootLogger = loggerFactory.CreateLogger(categoryType);
        CategoryType = categoryType;
        Template = "[{Time}] [{LogLevel}] {Message}";
    }

    internal static ExtendableLogger Create<T>(ILoggerFactory loggerFactory,
        string[] subSpace) where T : notnull
    {
        return new ExtendableLogger(loggerFactory, subSpace, typeof(T));
    }

    internal static ExtendableLogger Create<T>(ILoggerFactory loggerFactory)
        where T : notnull
    {
        return new ExtendableLogger(loggerFactory, typeof(T));
    }

    internal static ExtendableLogger Create(ILoggerFactory loggerFactory, Type type)
    {
        return new ExtendableLogger(loggerFactory, type);
    }

    internal static ExtendableLogger Create(ILoggerFactory loggerFactory, string[] subSpace, Type type)
    {
        return new ExtendableLogger(loggerFactory, subSpace, type);
    }

    public override InternalILogger Extend(string subSpace)
    {
        return new ExtendableLogger(LoggerFactory, [..Tags, subSpace], CategoryType);
    }

    public override InternalILogger Extend(string[] subSpace)
    {
        return new ExtendableLogger(LoggerFactory, [..Tags, ..subSpace], CategoryType);
    }
}