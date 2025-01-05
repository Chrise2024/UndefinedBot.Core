namespace UndefinedBot.Core.Utils;

public enum UndefinedLogLevel
{
    Trace,
    Debug,
    Information,
    Warning,
    Error,
    Critical,
    None,
}

public delegate void CommonLogEventHandler(string timeString, UndefinedLogLevel undefinedLogLevel, string message,
    string template,
    string[] tags);

public delegate void ExceptionLogEventHandler(string timeString, UndefinedLogLevel undefinedLogLevel, Exception? ex,
    string message,
    string template, string[] tags);

internal static class LogEventBus
{
    private static event CommonLogEventHandler? CommonLogEvent;
    private static event ExceptionLogEventHandler? ExceptionLogEvent;

    internal static void RegisterCommonLogEventHandler(CommonLogEventHandler handler)
    {
        CommonLogEvent += handler;
    }

    internal static void RegisterExceptionLogEventHandler(ExceptionLogEventHandler handler)
    {
        ExceptionLogEvent += handler;
    }

    public static void SendLogMessage(UndefinedLogLevel undefinedLogLevel,
        string message,
        string template, string[] tags)
    {
        CommonLogEvent?.Invoke(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), undefinedLogLevel, message, template,
            tags);
    }

    public static void SendLogMessageWithException(UndefinedLogLevel undefinedLogLevel, Exception? ex,
        string message,
        string template, string[] tags)
    {
        ExceptionLogEvent?.Invoke(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), undefinedLogLevel, ex, message,
            template, tags);
    }
}

public interface ILogger
{
    string Template { get; }
    string[] Tags { get; }
    void Error(string message);
    void Error(Exception? ex, string message);
    void Warn(string message);
    void Warn(Exception? ex, string message);
    void Info(string message);
    void Info(Exception? ex, string message);
    void Debug(string message);
    void Debug(Exception? ex, string message);
    ILogger GetSubLogger(string subSpace);
    ILogger GetSubLogger(IEnumerable<string> subSpace);
}

internal sealed class BaseLogger : ILogger
{
    public string Template { get; }
    public string[] Tags { get; }

    internal BaseLogger(IEnumerable<string> nameSpace)
    {
        Tags = nameSpace.ToArray();
        string extendTemplate = "";
        for (int i = 0; i < Tags.Length; i++)
        {
            extendTemplate += $"[{{Tag{i}}}] ";
        }

        Template = "[{Time}] " + extendTemplate + "[{LogLevel}] {Message}";
    }

    internal BaseLogger(string nameSpace) : this([nameSpace])
    {
    }

    private BaseLogger(ILogger cInst, string subSpace) : this(cInst, [subSpace])
    {
    }

    private BaseLogger(ILogger cInst, IEnumerable<string> subSpace) : this([..cInst.Tags, ..subSpace])
    {
    }

    public void Error(string message)
    {
        LogEventBus.SendLogMessage(UndefinedLogLevel.Error, message, Template, Tags);
    }

    public void Warn(string message)
    {
        LogEventBus.SendLogMessage(UndefinedLogLevel.Warning, message, Template, Tags);
    }

    public void Info(string message)
    {
        LogEventBus.SendLogMessage(UndefinedLogLevel.Information, message, Template, Tags);
    }

    public void Debug(string message)
    {
        LogEventBus.SendLogMessage(UndefinedLogLevel.Debug, message, Template, Tags);
    }

    public void Error(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(UndefinedLogLevel.Error, ex, message, Template, Tags);
    }

    public void Warn(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(UndefinedLogLevel.Error, ex, message, Template, Tags);
    }

    public void Info(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(UndefinedLogLevel.Error, ex, message, Template, Tags);
    }

    public void Debug(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(UndefinedLogLevel.Error, ex, message, Template, Tags);
    }

    public ILogger GetSubLogger(string subSpace)
    {
        return new BaseLogger(this, subSpace);
    }

    public ILogger GetSubLogger(IEnumerable<string> subSpace)
    {
        return new BaseLogger(this, subSpace);
    }
}