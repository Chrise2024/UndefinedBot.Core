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
    ITopLevelLogger GetSubLogger(string subName);
}
public interface ITopLevelLogger
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

internal sealed class GeneralLogger : ILogger
{
    private readonly string _nameSpace;

    internal GeneralLogger(string nameSpace)
    {
        _nameSpace = nameSpace;
    }

    public void Error(string message)
    {
        LogEventBus.SendLogMessage(_nameSpace, UndefinedLogLevel.Error, message);
    }

    public void Warn(string message)
    {
        LogEventBus.SendLogMessage(_nameSpace, UndefinedLogLevel.Warning, message);
    }

    public void Info(string message)
    {
        LogEventBus.SendLogMessage(_nameSpace, UndefinedLogLevel.Information, message);
    }

    public void Debug(string message)
    {
        LogEventBus.SendLogMessage(_nameSpace, UndefinedLogLevel.Debug, message);
    }

    public void Error(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(_nameSpace, UndefinedLogLevel.Error, ex, message);
    }

    public void Warn(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(_nameSpace, UndefinedLogLevel.Error, ex, message);
    }

    public void Info(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(_nameSpace, UndefinedLogLevel.Error, ex, message);
    }

    public void Debug(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(_nameSpace, UndefinedLogLevel.Error, ex, message);
    }
    public ITopLevelLogger GetSubLogger(string subName)
    {
        throw new NotSupportedException("This Logger Not Support Sub Logger");
    }
}
internal sealed class GeneralTopLevelFeatureLogger : ITopLevelLogger
{
    private readonly string _nameSpace;
    private readonly string _subSpace;

    internal GeneralTopLevelFeatureLogger(string nameSpace,string subSpace)
    {
        _nameSpace = nameSpace;
        _subSpace = subSpace;
    }

    public void Error(string message)
    {
        LogEventBus.SendLogMessage(_nameSpace, _subSpace, UndefinedLogLevel.Error, message);
    }

    public void Warn(string message)
    {
        LogEventBus.SendLogMessage(_nameSpace, _subSpace, UndefinedLogLevel.Warning, message);
    }

    public void Info(string message)
    {
        LogEventBus.SendLogMessage(_nameSpace, _subSpace, UndefinedLogLevel.Information, message);
    }

    public void Debug(string message)
    {
        LogEventBus.SendLogMessage(_nameSpace, _subSpace, UndefinedLogLevel.Debug, message);
    }

    public void Error(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(_nameSpace, _subSpace, UndefinedLogLevel.Error, ex, message);
    }

    public void Warn(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(_nameSpace, _subSpace, UndefinedLogLevel.Error, ex, message);
    }

    public void Info(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(_nameSpace, _subSpace, UndefinedLogLevel.Error, ex, message);
    }

    public void Debug(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException(_nameSpace, _subSpace, UndefinedLogLevel.Error, ex, message);
    }
}

internal sealed class CommandLogger : ITopLevelLogger
{
    private readonly string _pluginName;
    private readonly string _commandName;

    internal CommandLogger(string pluginName, string commandName)
    {
        _pluginName = pluginName;
        _commandName = commandName;
    }

    public void Error(string message)
    {
        LogEventBus.SendLogMessage("Command", _pluginName, _commandName, UndefinedLogLevel.Error, message);
    }

    public void Warn(string message)
    {
        LogEventBus.SendLogMessage("Command", _pluginName, _commandName, UndefinedLogLevel.Warning, message);
    }

    public void Info(string message)
    {
        LogEventBus.SendLogMessage("Command", _pluginName, _commandName, UndefinedLogLevel.Information, message);
    }

    public void Debug(string message)
    {
        LogEventBus.SendLogMessage("Command", _pluginName, _commandName, UndefinedLogLevel.Debug, message);
    }

    public void Error(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Command", _pluginName, _commandName, UndefinedLogLevel.Error, ex, message);
    }

    public void Warn(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Command", _pluginName, _commandName, UndefinedLogLevel.Error, ex, message);
    }

    public void Info(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Command", _pluginName, _commandName, UndefinedLogLevel.Error, ex, message);
    }

    public void Debug(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Command", _pluginName, _commandName, UndefinedLogLevel.Error, ex, message);
    }
}

internal sealed class AdapterLogger : ILogger
{
    private readonly string _adapterName;

    internal AdapterLogger(string adapterName)
    {
        _adapterName = adapterName;
    }

    public void Error(string message)
    {
        LogEventBus.SendLogMessage("Adapter", _adapterName, UndefinedLogLevel.Error, message);
    }

    public void Warn(string message)
    {
        LogEventBus.SendLogMessage("Adapter", _adapterName, UndefinedLogLevel.Warning, message);
    }

    public void Info(string message)
    {
        LogEventBus.SendLogMessage("Adapter", _adapterName, UndefinedLogLevel.Information, message);
    }

    public void Debug(string message)
    {
        LogEventBus.SendLogMessage("Adapter", _adapterName, UndefinedLogLevel.Debug, message);
    }

    public void Error(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Adapter", _adapterName, UndefinedLogLevel.Error, ex, message);
    }

    public void Warn(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Adapter", _adapterName, UndefinedLogLevel.Error, ex, message);
    }

    public void Info(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Adapter", _adapterName, UndefinedLogLevel.Error, ex, message);
    }

    public void Debug(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Adapter", _adapterName, UndefinedLogLevel.Error, ex, message);
    }
    public ITopLevelLogger GetSubLogger(string subName)
    {
        return new AdapterSubFeatureLogger(_adapterName, subName);
    }
}

internal sealed class AdapterSubFeatureLogger : ITopLevelLogger
{
    private readonly string _adapterName;
    private readonly string _subName;

    internal AdapterSubFeatureLogger(string adapterName, string subName)
    {
        _adapterName = adapterName;
        _subName = subName;
    }

    public void Error(string message)
    {
        LogEventBus.SendLogMessage("Adapter", _adapterName, _subName, UndefinedLogLevel.Error, message);
    }

    public void Warn(string message)
    {
        LogEventBus.SendLogMessage("Adapter", _adapterName, _subName, UndefinedLogLevel.Warning, message);
    }

    public void Info(string message)
    {
        LogEventBus.SendLogMessage("Adapter", _adapterName, _subName, UndefinedLogLevel.Information, message);
    }

    public void Debug(string message)
    {
        LogEventBus.SendLogMessage("Adapter", _adapterName, _subName, UndefinedLogLevel.Debug, message);
    }

    public void Error(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Adapter", _adapterName, _subName, UndefinedLogLevel.Error, ex, message);
    }

    public void Warn(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Adapter", _adapterName, _subName, UndefinedLogLevel.Error, ex, message);
    }

    public void Info(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Adapter", _adapterName, _subName, UndefinedLogLevel.Error, ex, message);
    }

    public void Debug(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Adapter", _adapterName, _subName, UndefinedLogLevel.Error, ex, message);
    }
}

internal sealed class PluginLogger : ILogger
{
    private readonly string _pluginName;

    internal PluginLogger(string pluginName)
    {
        _pluginName = pluginName;
    }

    public void Error(string message)
    {
        LogEventBus.SendLogMessage("Plugin", _pluginName, UndefinedLogLevel.Error, message);
    }

    public void Warn(string message)
    {
        LogEventBus.SendLogMessage("Plugin", _pluginName, UndefinedLogLevel.Warning, message);
    }

    public void Info(string message)
    {
        LogEventBus.SendLogMessage("Plugin", _pluginName, UndefinedLogLevel.Information, message);
    }

    public void Debug(string message)
    {
        LogEventBus.SendLogMessage("Plugin", _pluginName, UndefinedLogLevel.Debug, message);
    }

    public void Error(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Plugin", _pluginName, UndefinedLogLevel.Error, ex, message);
    }

    public void Warn(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Plugin", _pluginName, UndefinedLogLevel.Error, ex, message);
    }

    public void Info(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Plugin", _pluginName, UndefinedLogLevel.Error, ex, message);
    }

    public void Debug(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Plugin", _pluginName, UndefinedLogLevel.Error, ex, message);
    }
    public ITopLevelLogger GetSubLogger(string subName)
    {
        return new PluginSubFeatureLogger(_pluginName, subName);
    }
}

internal sealed class PluginSubFeatureLogger : ITopLevelLogger
{
    private readonly string _pluginName;
    private readonly string _subName;

    internal PluginSubFeatureLogger(string pluginName, string subName)
    {
        _pluginName = pluginName;
        _subName = subName;
    }

    public void Error(string message)
    {
        LogEventBus.SendLogMessage("Plugin", _pluginName, _subName, UndefinedLogLevel.Error, message);
    }

    public void Warn(string message)
    {
        LogEventBus.SendLogMessage("Plugin", _pluginName, _subName, UndefinedLogLevel.Warning, message);
    }

    public void Info(string message)
    {
        LogEventBus.SendLogMessage("Plugin", _pluginName, _subName, UndefinedLogLevel.Information, message);
    }

    public void Debug(string message)
    {
        LogEventBus.SendLogMessage("Plugin", _pluginName, _subName, UndefinedLogLevel.Debug, message);
    }

    public void Error(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Plugin", _pluginName, _subName, UndefinedLogLevel.Error, ex, message);
    }

    public void Warn(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Plugin", _pluginName, _subName, UndefinedLogLevel.Error, ex, message);
    }

    public void Info(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Plugin", _pluginName, _subName, UndefinedLogLevel.Error, ex, message);
    }

    public void Debug(Exception? ex, string message)
    {
        LogEventBus.SendLogMessageWithException("Plugin", _pluginName, _subName, UndefinedLogLevel.Error, ex, message);
    }
}