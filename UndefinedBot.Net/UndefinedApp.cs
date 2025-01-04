using System.Text.Json;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;
using UndefinedBot.Net.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Plugin;

namespace UndefinedBot.Net;

public class UndefinedApp(IHost host) : IHost
{
    public IServiceProvider Services => HostApp.Services;
    private IHost HostApp => host;
    private ILogger<UndefinedApp> Logger => Services.GetRequiredService<ILogger<UndefinedApp>>();
    private IConfiguration Configuration => Services.GetRequiredService<IConfiguration>();

    private readonly string _programRoot = Environment.CurrentDirectory;

    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true, IndentSize = 4 };

    private LogLevel ConvertLogLevel(UndefinedLogLevel ul)
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

    public async Task StartAsync(CancellationToken cancellationToken = new())
    {
        //Add LogEvent Handler
        LogEventBus.RegisterCommonLogEventHandler((time, undefinedLogLevel, message, template, tags) =>
        {
            Logger.Log(
                ConvertLogLevel(undefinedLogLevel),
                template,
                [time, ..tags, undefinedLogLevel.ToString(), message]
            );
        });
        LogEventBus.RegisterExceptionLogEventHandler((time, undefinedLogLevel, ex, message, template, tags) =>
        {
            Logger.Log(
                ConvertLogLevel(undefinedLogLevel),
                ex,
                template,
                [time, ..tags, undefinedLogLevel.ToString(), message]
            );
        });
        //Load Root Config
        RootConfigManager.InitConfig(new RootConfigData
        {
            GroupId = Configuration.GetSection("GroupId").GetChildren().Select(child => long.Parse(child.Value!))
                .ToList(),
            CommandPrefix = Configuration["CommandPrefix"]!
        });
        //Load Adapter and Plugin
        Init();

        await HostApp.StartAsync(cancellationToken);

        Logger.LogInformation("UndefinedBot.Net Implementation has started");
        //for test
        CommandEventBus.InvokeCommandEvent(
            CommandInvokeProperties.Group(
                    "test",
                    0,
                    0,
                    0)
                .Implement(
                    "OneBot11Adapter",
                    "",
                    "",
                    []
                ),
            UserCommandSource.Friend(0, "", 0)
        );
        //Console Loop
        while (true)
        {
            string? tempString = Console.ReadLine();
            if (tempString == "stop")
            {
                break;
            }

            if (tempString == "reload")
            {
                Init();
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = new())
    {
        await HostApp.StopAsync(cancellationToken);

        Logger.LogInformation("UndefinedBot.Net Implementation has stopped");
    }

    public void Dispose()
    {
        HostApp.Dispose();
        GC.SuppressFinalize(this);
    }

    private void Init()
    {
        //Load Adapters
        Dictionary<string, IAdapterInstance> adapterReference = AdapterLoader.LoadAdapters();
        //Load Plugins
        List<IPluginInstance> pluginReference = PluginLoader.LoadPlugins();
        //Get Command References for Help command
        Dictionary<string, CommandProperties> commandReference = PluginLoader.GetCommandReference();

        FileIO.WriteAsJson(Path.Join(_programRoot, "adapter_reference.json"), adapterReference);

        FileIO.WriteAsJson(Path.Join(_programRoot, "plugin_reference.json"), pluginReference);

        FileIO.WriteAsJson(Path.Join(_programRoot, "command_reference.json"), commandReference);

        Logger.LogInformation("Loaded Adapters:{AdapterList}",
            JsonSerializer.Serialize(adapterReference.Select(item => item.Value), _serializerOptions));

        Logger.LogInformation("Loaded Plugins:{PluginList}",
            JsonSerializer.Serialize(pluginReference.Select(item => item), _serializerOptions));
    }
}