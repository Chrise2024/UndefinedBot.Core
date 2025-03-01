using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UndefinedBot.Core.Utils.Logging;

namespace UndefinedBot.Net.Utils;

internal sealed class LogService(ILogger<LogService> logger) : BackgroundService,IDisposable
{
    private Task? LoggerTask { get; set; }
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            LogMessage lm = await LogEventBus.ReadLogMessageAsync(token);
            logger.Log(
                ConvertLogLevel(lm.LogLevel),
                lm.Exception,
                lm.Template,
                lm.Content
            );
        }
    }
    private static LogLevel ConvertLogLevel(UndefinedLogLevel ul)
    {
        return ul switch
        {
            UndefinedLogLevel.Trace => LogLevel.Trace,
            UndefinedLogLevel.Debug => LogLevel.Debug,
            UndefinedLogLevel.Information => LogLevel.Information,
            UndefinedLogLevel.Warning => LogLevel.Warning,
            UndefinedLogLevel.Error => LogLevel.Error,
            UndefinedLogLevel.Critical => LogLevel.Critical,
            UndefinedLogLevel.None => LogLevel.None,
            _ => LogLevel.Error,
        };
    }

    public void Dispose()
    {
        _cancellationTokenSource.Dispose();
    }
}