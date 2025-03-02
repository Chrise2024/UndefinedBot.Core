using Microsoft.Extensions.Logging;
using MsILogger = Microsoft.Extensions.Logging.ILogger;
using InternalILogger = UndefinedBot.Core.Utils.ILogger;

namespace UndefinedBot.Net.Utils.Logging;

public abstract class BaseLogger : InternalILogger
{
    protected abstract string Template { get; }

    protected abstract string[] Tags { get; }

    protected abstract MsILogger RootLogger { get; }

    public void Critical(string message)
    {
        RootLogger.LogCritical(Template, [GetTimeString(), "Critical", ..Tags, message]);
    }

    public void Critical(Exception? ex, string message)
    {
        RootLogger.LogCritical(ex, Template, [GetTimeString(), "Critical", ..Tags, message]);
    }

    public void Error(string message)
    {
        RootLogger.LogError(Template, [GetTimeString(), "Error", ..Tags, message]);
    }

    public void Error(Exception? ex, string message)
    {
        RootLogger.LogError(ex, Template, [GetTimeString(), "Error", ..Tags, message]);
    }

    public void Warn(string message)
    {
        RootLogger.LogWarning(Template, [GetTimeString(), "Warn", ..Tags, message]);
    }

    public void Warn(Exception? ex, string message)
    {
        RootLogger.LogWarning(ex, Template, [GetTimeString(), "Warn", ..Tags, message]);
    }

    public void Info(string message)
    {
        RootLogger.LogInformation(Template, [GetTimeString(), "Info", ..Tags, message]);
    }

    public void Info(Exception? ex, string message)
    {
        RootLogger.LogInformation(ex, Template, [GetTimeString(), "Info", ..Tags, message]);
    }

    public void Debug(string message)
    {
        RootLogger.LogDebug(Template, [GetTimeString(), "Debug", ..Tags, message]);
    }

    public void Debug(Exception? ex, string message)
    {
        RootLogger.LogDebug(ex, Template, [GetTimeString(), "Debug", ..Tags, message]);
    }

    public void Trace(string message)
    {
        RootLogger.LogTrace(Template, [GetTimeString(), "Trace", ..Tags, message]);
    }

    public void Trace(Exception? ex, string message)
    {
        RootLogger.LogTrace(ex, Template, [GetTimeString(), "Trace", ..Tags, message]);
    }

    public abstract InternalILogger Extend(string subSpace);
    public abstract InternalILogger Extend(string[] subSpace);

    private static string GetTimeString()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}