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
        //Load Adapter and Plugin
        Init();

        await HostApp.StartAsync(cancellationToken);

        Logger.LogInformation("UndefinedBot.Net Implementation has started");
        //for test
        var r = await CommandInvokeManager.InvokeCommand(
            CommandInvokeProperties.Group(
                    "test",
                    0,
                    0,
                    114514191)
                .Implement(
                    "OneBot11Adapter",
                    "",
                    "",
                    []
                ),
            UserCommandSource.Friend(0, "", 0)
        );
        //Console.WriteLine(r);
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
        List<IAdapterInstance> adapterList = AdapterLoader.LoadAdapters();
        //Load Plugins
        List<IPluginInstance> pluginList = PluginLoader.LoadPlugins();
        //Get Command References for Help command
        Dictionary<string, CommandProperties> commandReference = PluginLoader.GetCommandReference();
        string pluginListText = JsonSerializer.Serialize(pluginList, _serializerOptions);
        string adapterListText = JsonSerializer.Serialize(adapterList, _serializerOptions);
        string commandReferenceText = JsonSerializer.Serialize(commandReference, _serializerOptions);

        FileIO.WriteFile(Path.Join(_programRoot, "loaded_adapters.json"), adapterListText);

        FileIO.WriteFile(Path.Join(_programRoot, "loaded_plugins.json"), pluginListText);

        FileIO.WriteFile(Path.Join(_programRoot, "command_reference.json"), commandReferenceText);

        Logger.LogInformation("Loaded Adapters:{AdapterList}", adapterListText);

        Logger.LogInformation("Loaded Plugins:{PluginList}", pluginListText);

        Logger.LogInformation("Loaded Commands:{CommandList}", commandReferenceText);
    }
}