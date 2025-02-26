namespace UndefinedBot.Core.Utils.Logging;

public abstract class BaseLogger : IDisposable
{
    protected abstract string Template { get; }

    protected abstract string[] Tags { get; }

    internal void Critical(string message)
    {
        LogEventBus.SendLogMessage(UndefinedLogLevel.Critical, message, Template, Tags);
    }

    internal void Critical(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(UndefinedLogLevel.Critical, ex, message, Template, Tags);
    }

    public void Error(string message)
    {
        LogEventBus.SendLogMessage(UndefinedLogLevel.Error, message, Template, Tags);
    }

    public void Error(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(UndefinedLogLevel.Error, ex, message, Template, Tags);
    }

    public void Warn(string message)
    {
        LogEventBus.SendLogMessage(UndefinedLogLevel.Warning, message, Template, Tags);
    }

    public void Warn(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(UndefinedLogLevel.Warning, ex, message, Template, Tags);
    }

    public void Info(string message)
    {
        LogEventBus.SendLogMessage(UndefinedLogLevel.Information, message, Template, Tags);
    }

    public void Info(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(UndefinedLogLevel.Information, ex, message, Template, Tags);
    }

    public void Debug(string message)
    {
        LogEventBus.SendLogMessage(UndefinedLogLevel.Debug, message, Template, Tags);
    }

    public void Debug(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(UndefinedLogLevel.Debug, ex, message, Template, Tags);
    }

    public void Trace(string message)
    {
        LogEventBus.SendLogMessage(UndefinedLogLevel.Trace, message, Template, Tags);
    }

    public void Trace(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(UndefinedLogLevel.Trace, ex, message, Template, Tags);
    }

    public void Dispose()
    {
        Array.Clear(Tags);
        GC.SuppressFinalize(this);
    }
}