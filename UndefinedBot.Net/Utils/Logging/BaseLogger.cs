using Microsoft.Extensions.Logging;
using ILogger = UndefinedBot.Core.Utils.ILogger;

namespace UndefinedBot.Net.Utils.Logging;

public abstract class BaseLogger : ILogger
{
    protected abstract string Template { get; }

    protected abstract string[] Tags { get; }

    protected abstract ILogger<BaseLogger> RootLogger { get; }

    public void Critical(string message)
    {
        RootLogger.LogCritical(Template, [GetTimeString(), ..Tags, "Critical", message]);
    }

    public void Critical(Exception? ex, string message)
    {
        RootLogger.LogCritical(ex, Template, [GetTimeString(), ..Tags, "Critical", message]);
    }

    public void Error(string message)
    {
        RootLogger.LogError(Template, [GetTimeString(), ..Tags, "Error", message]);
    }

    public void Error(Exception? ex, string message)
    {
        RootLogger.LogError(ex, Template, [GetTimeString(), ..Tags, "Error", message]);
    }

    public void Warn(string message)
    {
        RootLogger.LogWarning(Template, [GetTimeString(), ..Tags, "Warn", message]);
    }

    public void Warn(Exception? ex, string message)
    {
        RootLogger.LogWarning(ex, Template, [GetTimeString(), ..Tags, "Warn", message]);
    }

    public void Info(string message)
    {
        RootLogger.LogInformation(Template, [GetTimeString(), ..Tags, "Info", message]);
    }

    public void Info(Exception? ex, string message)
    {
        RootLogger.LogInformation(ex, Template, [GetTimeString(), ..Tags, "Info", message]);
    }

    public void Debug(string message)
    {
        RootLogger.LogDebug(Template, [GetTimeString(), ..Tags, "Debug", message]);
    }

    public void Debug(Exception? ex, string message)
    {
        RootLogger.LogDebug(ex, Template, [GetTimeString(), ..Tags, "Debug", message]);
    }

    public void Trace(string message)
    {
        RootLogger.LogTrace(Template, [GetTimeString(), ..Tags, "Trace", message]);
    }

    public void Trace(Exception? ex, string message)
    {
        RootLogger.LogTrace(ex, Template, [GetTimeString(), ..Tags, "Trace", message]);
    }

    public abstract ILogger Extend(string subSpace);
    public abstract ILogger Extend(IEnumerable<string> subSpace);

    private static string GetTimeString()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}