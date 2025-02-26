namespace UndefinedBot.Core.Utils.Logging;

internal static class LogEventBus
{
    private static CommonLogEventHandler _commonLogEvent =
        (_, _, _) => Console.WriteLine("CommonLogEventHandler Undefined!!!");

    private static ExceptionLogEventHandler _exceptionLogEvent =
        (_, _, _, _) => Console.WriteLine("ExceptionLogEventHandler Undefined!!!");

    public static void RegisterCommonLogEventHandler(CommonLogEventHandler handler)
    {
        _commonLogEvent = handler;
    }

    public static void RegisterExceptionLogEventHandler(ExceptionLogEventHandler handler)
    {
        _exceptionLogEvent = handler;
    }

    public static void SendLogMessage(UndefinedLogLevel undefinedLogLevel,
        string message,
        string template, string[] tags)
    {
        _commonLogEvent.Invoke(undefinedLogLevel,template,
            [GetTimeString(),..tags,undefinedLogLevel,message]);
    }

    public static void SendLogMessageWithException(UndefinedLogLevel undefinedLogLevel, Exception? ex,
        string message,
        string template, string[] tags)
    {
        _exceptionLogEvent.Invoke(undefinedLogLevel, ex,
            template, [GetTimeString(),..tags,undefinedLogLevel,message]);
    }

    private static string GetTimeString() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
}

public delegate void CommonLogEventHandler(UndefinedLogLevel undefinedLogLevel, string template, object[] content);

public delegate void ExceptionLogEventHandler(UndefinedLogLevel undefinedLogLevel, Exception? ex, string template, object[] content);

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