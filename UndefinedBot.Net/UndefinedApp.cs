using UndefinedBot.Core.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UndefinedBot.Net;

public class UndefinedApp(IHost host) : IHost
{
    private IHost _hostApp => host;
    public IServiceProvider Services => _hostApp.Services;
    public ILogger<UndefinedApp> Logger => Services.GetRequiredService<ILogger<UndefinedApp>>();

    public IConfiguration Configuration => Services.GetRequiredService<IConfiguration>();

    public async Task StartAsync(CancellationToken cancellationToken = new())
    {
        await _hostApp.StartAsync(cancellationToken);
        LogEventBus.CoreLogEvent += (nameSpace, subSpace, undefinedLogLevel, message) =>
        {
            Logger.Log(undefinedLogLevel switch
            {
                UndefinedLogLevel.Trace => LogLevel.Trace,
                UndefinedLogLevel.Debug => LogLevel.Debug,
                UndefinedLogLevel.Warning => LogLevel.Warning,
                UndefinedLogLevel.Error => LogLevel.Error,
                UndefinedLogLevel.Critical => LogLevel.Critical,
                UndefinedLogLevel.None => LogLevel.None,
                _ => LogLevel.Error,
            },
            $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{nameSpace}] [{subSpace}] [{undefinedLogLevel.ToString()}] [{message}]");
        };
        Logger.LogInformation("UndefinedBot.Net Implementation has started");
    }

    public async Task StopAsync(CancellationToken cancellationToken = new())
    {
        await _hostApp.StopAsync(cancellationToken);
        Logger.LogInformation("UndefinedBot.Net Implementation has stopped");
    }
    public void Dispose()
    {
        _hostApp.Dispose();
        GC.SuppressFinalize(this);
    }
}
