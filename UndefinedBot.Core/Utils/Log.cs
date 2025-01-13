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

public interface ILogger : IDisposable
{
    string Template { get; }
    string[] Tags { get; }
    internal void Critical(string message);
    internal void Critical(Exception? ex, string message);
    void Error(string message);
    void Error(Exception? ex, string message);
    void Warn(string message);
    void Warn(Exception? ex, string message);
    void Info(string message);
    void Info(Exception? ex, string message);
    void Debug(string message);
    void Debug(Exception? ex, string message);
    void Trace(string message);
    void Trace(Exception? ex, string message);
    ILogger GetSubLogger(string subSpace);
    ILogger GetSubLogger(IEnumerable<string> subSpace);
}

public sealed class ExtendableLogger : ILogger
{
    public string Template { get; }
    public string[] Tags { get; }

    internal ExtendableLogger(IEnumerable<string> nameSpace)
    {
        Tags = nameSpace.ToArray();
        string extendTemplate = "";
        for (int i = 0; i < Tags.Length; i++)
        {
            extendTemplate += $"[{{Tag{i}}}] ";
        }

        Template = "[{Time}] " + extendTemplate + "[{LogLevel}] {Message}";
    }

    internal ExtendableLogger(string nameSpace) : this([nameSpace])
    {
    }

    private ExtendableLogger(ILogger cInst, string subSpace) : this(cInst, [subSpace])
    {
    }

    private ExtendableLogger(ILogger cInst, IEnumerable<string> subSpace) : this([..cInst.Tags, ..subSpace])
    {
    }

    public void Critical(string message)
    {
        LogEventBus.SendLogMessage(UndefinedLogLevel.Critical, message, Template, Tags);
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
    
    public void Trace(string message)
    {
        LogEventBus.SendLogMessage(UndefinedLogLevel.Trace, message, Template, Tags);
    }

    public void Critical(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(UndefinedLogLevel.Critical, ex, message, Template, Tags);
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
    
    public void Trace(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(UndefinedLogLevel.Trace, ex, message, Template, Tags);
    }

    public ILogger GetSubLogger(string subSpace)
    {
        return new ExtendableLogger(this, subSpace);
    }

    public ILogger GetSubLogger(IEnumerable<string> subSpace)
    {
        return new ExtendableLogger(this, subSpace);
    }
    public void Dispose()
    {
        Array.Clear(Tags);
    }
}

public sealed class FixedLogger : ILogger
{
    public string Template { get; }
    public string[] Tags { get; }

    internal FixedLogger(IEnumerable<string> nameSpace)
    {
        int i = 0;
        Tags = [..nameSpace];
        string extendTemplate = Tags.Aggregate("", (current, _) => current + $"[{{Tag{i++}}}] ");

        Template = "[{Time}] " + extendTemplate + "[{LogLevel}] {Message}";
    }

    internal FixedLogger(string nameSpace) : this([nameSpace])
    {
    }

    public void Critical(string message)
    {
        LogEventBus.SendLogMessage(UndefinedLogLevel.Critical, message, Template, Tags);
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
    
    public void Trace(string message)
    {
        LogEventBus.SendLogMessage(UndefinedLogLevel.Trace, message, Template, Tags);
    }

    public void Critical(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(UndefinedLogLevel.Critical, ex, message, Template, Tags);
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
    
    public void Trace(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(UndefinedLogLevel.Trace, ex, message, Template, Tags);
    }

    ILogger ILogger.GetSubLogger(string subSpace)
    {
        throw new NotSupportedException("This Logger cannot be extended");
    }

    ILogger ILogger.GetSubLogger(IEnumerable<string> subSpace)
    {
        throw new NotSupportedException("This Logger cannot be extended");
    }
    public void Dispose()
    {
        Array.Clear(Tags);
    }
}