using System.Text.Json;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;
using UndefinedBot.Net.Utils;
using UndefinedBot.Net.NetWork;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UndefinedBot.Net;

public class UndefinedApp(IHost host) : IHost
{
    public IServiceProvider Services => HostApp.Services;
    private IHost HostApp => host;
    private ILogger<UndefinedApp> Logger => Services.GetRequiredService<ILogger<UndefinedApp>>();
    //private HttpServer HttpServer => Services.GetRequiredService<HttpServer>();
    private NetworkServiceCollection NetworkService => Services.GetRequiredService<NetworkServiceCollection>();
    private IConfiguration Configuration => Services.GetRequiredService<IConfiguration>();

    private readonly string _programRoot = Environment.CurrentDirectory;

    private List<PluginMetaProperties> _pluginReference = [];

    private Dictionary<string, CommandMetaProperties> _commandReference = [];

    private Dictionary<string, CommandInstance> _commandInstance = [];

    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true,IndentSize = 4 };

    public async Task StartAsync(CancellationToken cancellationToken = new())
    {
        LogEventBus.RegisterLogEventHandler((nameSpace, subSpace, undefinedLogLevel, message) =>
        {
            Services.GetRequiredService<ILogger<UndefinedApp>>().Log(undefinedLogLevel switch
            {
                UndefinedLogLevel.Trace => LogLevel.Trace,
                UndefinedLogLevel.Debug => LogLevel.Debug,
                UndefinedLogLevel.Information => LogLevel.Information,
                UndefinedLogLevel.Warning => LogLevel.Warning,
                UndefinedLogLevel.Error => LogLevel.Error,
                UndefinedLogLevel.Critical => LogLevel.Critical,
                UndefinedLogLevel.None => LogLevel.None,
                _ => LogLevel.Error,
            },
            "[{Time}] [{nameSpace}] [{subSpace}] [{undefinedLogLevel.ToString()}] {message}",
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            nameSpace,
            subSpace,
            undefinedLogLevel.ToString(),
            message
            );
        });
        ConfigManager.InitConfig(new Config
        {
            HttpServer = new HttpServiceOptions(Configuration["HttpServer:Host"]!,Configuration["HttpServer:Port"]!,Configuration["HttpServer:AccessToken"]!),
            HttpPost = new HttpServiceOptions(Configuration["HttpPost:Host"]!,Configuration["HttpPost:Port"]!,Configuration["HttpServer:AccessToken"]!),
            GroupId = Configuration.GetSection("GroupId").GetChildren().Select(child => long.Parse(child.Value!)).ToList(),
            CommandPrefix = Configuration["CommandPrefix"]!
        });

        (_pluginReference,_commandInstance) = Initializer.LoadPlugins();

        _commandReference = Initializer.GetCommandReference();

        FileIO.WriteAsJson(Path.Join(_programRoot, "plugin_reference.json"),_pluginReference);

        FileIO.WriteAsJson(Path.Join(_programRoot, "command_reference.json"), _commandReference);

        await HostApp.StartAsync(cancellationToken);

        await NetworkService.StartAsync(cancellationToken);

        Logger.LogInformation("UndefinedBot.Net Implementation has started");

        Logger.LogInformation("Loaded Plugins:{PluginList}", JsonSerializer.Serialize(_pluginReference.Select(item => item.Name), _serializerOptions));
    }

    public async Task StopAsync(CancellationToken cancellationToken = new())
    {
        await HostApp.StopAsync(cancellationToken);

        await NetworkService.StopAsync(cancellationToken);

        Logger.LogInformation("UndefinedBot.Net Implementation has stopped");
    }
    public void Dispose()
    {
        HostApp.Dispose();
        GC.SuppressFinalize(this);
    }
}
