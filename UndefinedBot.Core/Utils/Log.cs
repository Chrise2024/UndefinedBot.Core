
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

public delegate void LogEventHandler(string nameSpace, string subSpace, UndefinedLogLevel undefinedLogLevel, string message);
internal abstract class LogEventBus
{
    internal static event LogEventHandler? CoreLogEvent;
    public static void SendLogMessage(string nameSpace, string subSpace, UndefinedLogLevel undefinedLogLevel, string message)
    {
        CoreLogEvent?.Invoke(nameSpace,subSpace,undefinedLogLevel,message);
    }
    public static void SendLogMessage(string nameSpace, UndefinedLogLevel undefinedLogLevel, string message)
    {
        CoreLogEvent?.Invoke(nameSpace,"Main",undefinedLogLevel,message);
    }
}
public class GeneralLogger(string nameSpace)
{
    public void Error(string message)
    {
        LogEventBus.SendLogMessage(nameSpace,UndefinedLogLevel.Error,message);
    }
    public void Warn(string message)
    {
        LogEventBus.SendLogMessage(nameSpace,UndefinedLogLevel.Warning,message);
    }
    public void Info(string message)
    {
        LogEventBus.SendLogMessage(nameSpace,UndefinedLogLevel.Information,message);
    }
    public void Debug(string message)
    {
        LogEventBus.SendLogMessage(nameSpace,UndefinedLogLevel.Debug,message);
    }
}
public class CommandLogger(string nameSpace,string commandName)
{
    public void Error(string message)
    {
        LogEventBus.SendLogMessage(nameSpace,commandName,UndefinedLogLevel.Error,message);
    }
    public void Warn(string message)
    {
        LogEventBus.SendLogMessage(nameSpace,commandName,UndefinedLogLevel.Warning,message);
    }
    public void Info(string message)
    {
        LogEventBus.SendLogMessage(nameSpace,commandName,UndefinedLogLevel.Information,message);
    }
    public void Debug(string message)
    {
        LogEventBus.SendLogMessage(nameSpace,commandName,UndefinedLogLevel.Debug,message);
    }
}
