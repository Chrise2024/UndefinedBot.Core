using System.Threading.Channels;

namespace UndefinedBot.Core.Utils.Logging;

internal static class LogEventBus
{
    private static readonly Channel<LogMessage> _logMessageChannel;
    static LogEventBus()
    {
        int cap = Core.RootConfig["BusCacheCapacity"]?["Logging"]?.GetValue<int>() ?? 128;
        _logMessageChannel = Channel.CreateBounded<LogMessage>(new BoundedChannelOptions(cap)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true
        });
    }

    public static void SendLogMessage(UndefinedLogLevel undefinedLogLevel,
        string message,
        string template, string[] tags)
    {
        LogMessage logMessage = new(undefinedLogLevel,
            template, [GetTimeString(), ..tags, undefinedLogLevel, message]);
        _logMessageChannel.Writer.TryWrite(logMessage);
    }

    public static void SendLogMessageWithException(UndefinedLogLevel undefinedLogLevel, Exception? ex,
        string message,
        string template, string[] tags)
    {
        LogMessage logMessage = new(undefinedLogLevel,
            template, [GetTimeString(), ..tags, undefinedLogLevel, message], ex);
        _logMessageChannel.Writer.TryWrite(logMessage);
    }
    public static async Task<LogMessage> ReadLogMessageAsync(CancellationToken token)
    {
        return await _logMessageChannel.Reader.ReadAsync(token);
    }

    private static string GetTimeString() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
}

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