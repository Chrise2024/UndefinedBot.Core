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
    private static string TemplateT3 => "[{Time}] [{Tag1}] [{Tag2}] [{Tag3}] [{LogLevel}] {Message}";
    private static string TemplateT2 => "[{Time}] [{Tag1}] [{Tag2}] [{LogLevel}] {Message}";
    private static string TemplateT1 => "[{Time}] [{tag1}] [{LogLevel}] {Message}";
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

    public static void SendLogMessage(string nameSpace1, string nameSpace2, string nameSpace3,
        UndefinedLogLevel undefinedLogLevel, string message)
    {
        CommonLogEvent?.Invoke(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), undefinedLogLevel, message, TemplateT3,
            [nameSpace1, nameSpace2, nameSpace3]);
    }

    public static void SendLogMessage(string nameSpace1, string nameSpace2, UndefinedLogLevel undefinedLogLevel,
        string message)
    {
        CommonLogEvent?.Invoke(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), undefinedLogLevel, message, TemplateT2,
            [nameSpace1, nameSpace2]);
    }

    public static void SendLogMessage(string nameSpace1, UndefinedLogLevel undefinedLogLevel, string message)
    {
        CommonLogEvent?.Invoke(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), undefinedLogLevel, message, TemplateT1,
            [nameSpace1]);
    }

    public static void SendLogMessageWithException(string nameSpace1, string nameSpace2, string nameSpace3,
        UndefinedLogLevel undefinedLogLevel, Exception? ex, string message)
    {
        ExceptionLogEvent?.Invoke(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), undefinedLogLevel, ex, message,
            TemplateT3, [nameSpace1, nameSpace2, nameSpace3]);
    }

    public static void SendLogMessageWithException(string nameSpace1, string nameSpace2,
        UndefinedLogLevel undefinedLogLevel, Exception? ex, string message)
    {
        ExceptionLogEvent?.Invoke(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), undefinedLogLevel, ex, message,
            TemplateT2, [nameSpace1, nameSpace2]);
    }

    public static void SendLogMessageWithException(string nameSpace1, UndefinedLogLevel undefinedLogLevel,
        Exception? ex, string message)
    {
        ExceptionLogEvent?.Invoke(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), undefinedLogLevel, ex, message,
            TemplateT1, [nameSpace1]);
    }
}

public interface ILogger
{
    void Error(string message);
    void Error(Exception? ex, string message);
    void Warn(string message);
    void Warn(Exception? ex, string message);
    void Info(string message);
    void Info(Exception? ex, string message);
    void Debug(string message);
    void Debug(Exception? ex, string message);
}

public sealed class GeneralLogger(string nameSpace) : ILogger
{
    public void Error(string message)
    {
        LogEventBus.SendLogMessage(nameSpace, UndefinedLogLevel.Error, message);
    }

    public void Warn(string message)
    {
        LogEventBus.SendLogMessage(nameSpace, UndefinedLogLevel.Warning, message);
    }

    public void Info(string message)
    {
        LogEventBus.SendLogMessage(nameSpace, UndefinedLogLevel.Information, message);
    }

    public void Debug(string message)
    {
        LogEventBus.SendLogMessage(nameSpace, UndefinedLogLevel.Debug, message);
    }

    public void Error(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(nameSpace, UndefinedLogLevel.Error, ex, message);
    }

    public void Warn(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(nameSpace, UndefinedLogLevel.Error, ex, message);
    }

    public void Info(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(nameSpace, UndefinedLogLevel.Error, ex, message);
    }

    public void Debug(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(nameSpace, UndefinedLogLevel.Error, ex, message);
    }
}

public sealed class CommandLogger(string pluginName, string commandName) : ILogger
{
    public void Error(string message)
    {
        LogEventBus.SendLogMessage("Command", pluginName, commandName, UndefinedLogLevel.Error, message);
    }

    public void Warn(string message)
    {
        LogEventBus.SendLogMessage("Command", pluginName, commandName, UndefinedLogLevel.Warning, message);
    }

    public void Info(string message)
    {
        LogEventBus.SendLogMessage("Command", pluginName, commandName, UndefinedLogLevel.Information, message);
    }

    public void Debug(string message)
    {
        LogEventBus.SendLogMessage("Command", pluginName, commandName, UndefinedLogLevel.Debug, message);
    }

    public void Error(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Command", pluginName, UndefinedLogLevel.Error, ex, message);
    }

    public void Warn(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Command", pluginName, UndefinedLogLevel.Error, ex, message);
    }

    public void Info(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Command", pluginName, UndefinedLogLevel.Error, ex, message);
    }

    public void Debug(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Command", pluginName, UndefinedLogLevel.Error, ex, message);
    }
}

public sealed class AdapterLogger(string adapterName) : ILogger
{
    public void Error(string message)
    {
        LogEventBus.SendLogMessage("Adapter", adapterName, UndefinedLogLevel.Error, message);
    }

    public void Warn(string message)
    {
        LogEventBus.SendLogMessage("Adapter", adapterName, UndefinedLogLevel.Warning, message);
    }

    public void Info(string message)
    {
        LogEventBus.SendLogMessage("Adapter", adapterName, UndefinedLogLevel.Information, message);
    }

    public void Debug(string message)
    {
        LogEventBus.SendLogMessage("Adapter", adapterName, UndefinedLogLevel.Debug, message);
    }

    public void Error(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Adapter", adapterName, UndefinedLogLevel.Error, ex, message);
    }

    public void Warn(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Adapter", adapterName, UndefinedLogLevel.Error, ex, message);
    }

    public void Info(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Adapter", adapterName, UndefinedLogLevel.Error, ex, message);
    }

    public void Debug(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Adapter", adapterName, UndefinedLogLevel.Error, ex, message);
    }
}
public sealed class AdapterSubFeatureLogger(string adapterName,string subName) : ILogger
{
    public void Error(string message)
    {
        LogEventBus.SendLogMessage("Adapter", adapterName,subName, UndefinedLogLevel.Error, message);
    }

    public void Warn(string message)
    {
        LogEventBus.SendLogMessage("Adapter", adapterName,subName, UndefinedLogLevel.Warning, message);
    }

    public void Info(string message)
    {
        LogEventBus.SendLogMessage("Adapter", adapterName,subName, UndefinedLogLevel.Information, message);
    }

    public void Debug(string message)
    {
        LogEventBus.SendLogMessage("Adapter", adapterName,subName, UndefinedLogLevel.Debug, message);
    }

    public void Error(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Adapter", adapterName,subName, UndefinedLogLevel.Error, ex, message);
    }

    public void Warn(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Adapter", adapterName,subName, UndefinedLogLevel.Error, ex, message);
    }

    public void Info(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Adapter", adapterName,subName, UndefinedLogLevel.Error, ex, message);
    }

    public void Debug(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Adapter", adapterName,subName, UndefinedLogLevel.Error, ex, message);
    }
}

public sealed class PluginLogger(string pluginName) : ILogger
{
    public void Error(string message)
    {
        LogEventBus.SendLogMessage("Plugin", pluginName, UndefinedLogLevel.Error, message);
    }

    public void Warn(string message)
    {
        LogEventBus.SendLogMessage("Plugin", pluginName, UndefinedLogLevel.Warning, message);
    }

    public void Info(string message)
    {
        LogEventBus.SendLogMessage("Plugin", pluginName, UndefinedLogLevel.Information, message);
    }

    public void Debug(string message)
    {
        LogEventBus.SendLogMessage("Plugin", pluginName, UndefinedLogLevel.Debug, message);
    }

    public void Error(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Plugin", pluginName, UndefinedLogLevel.Error, ex, message);
    }

    public void Warn(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Plugin", pluginName, UndefinedLogLevel.Error, ex, message);
    }

    public void Info(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Plugin", pluginName, UndefinedLogLevel.Error, ex, message);
    }

    public void Debug(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Plugin", pluginName, UndefinedLogLevel.Error, ex, message);
    }
}
public sealed class PluginSubFeatureLogger(string pluginName,string subname) : ILogger
{
    public void Error(string message)
    {
        LogEventBus.SendLogMessage("Plugin", pluginName,subname, UndefinedLogLevel.Error, message);
    }

    public void Warn(string message)
    {
        LogEventBus.SendLogMessage("Plugin", pluginName,subname, UndefinedLogLevel.Warning, message);
    }

    public void Info(string message)
    {
        LogEventBus.SendLogMessage("Plugin", pluginName,subname, UndefinedLogLevel.Information, message);
    }

    public void Debug(string message)
    {
        LogEventBus.SendLogMessage("Plugin", pluginName,subname, UndefinedLogLevel.Debug, message);
    }

    public void Error(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Plugin", pluginName,subname, UndefinedLogLevel.Error, ex, message);
    }

    public void Warn(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Plugin", pluginName,subname, UndefinedLogLevel.Error, ex, message);
    }

    public void Info(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Plugin", pluginName,subname, UndefinedLogLevel.Error, ex, message);
    }

    public void Debug(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Plugin", pluginName,subname, UndefinedLogLevel.Error, ex, message);
    }
}