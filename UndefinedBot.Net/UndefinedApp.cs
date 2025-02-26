using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Utils.Logging;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.Arguments.TokenContentType;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Net.Utils;
using UndefinedBot.Core.NetWork;
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

    public async Task StartAsync(CancellationToken cancellationToken = new())
    {
        //Add LogEvent Handler
        LogEventBus.RegisterCommonLogEventHandler((undefinedLogLevel, template, content) =>
        {
            Logger.Log(
                ConvertLogLevel(undefinedLogLevel),
                template,
                content
            );
        });
        LogEventBus.RegisterExceptionLogEventHandler((undefinedLogLevel, ex,  template, content) =>
        {
            Logger.Log(
                ConvertLogLevel(undefinedLogLevel),
                ex,
                template,
                content
            );
        });
        Logger.LogTrace("LogEventBus Registered");
        //Load Adapter and Plugin
        Init();

        await HostApp.StartAsync(cancellationToken);

        Logger.LogInformation("UndefinedBot.Net Implementation has started");
        //for test
        _ = await CommandManager.InvokeCommand(
            CommandBackgroundEnvironment.Group(
                    "help",
                    "0",
                    "0",
                    "0",
                    114514191)
                .Implement(
                    "OneBot11Adapter",
                    "",
                    "",
                    [
                        new ParsedToken(ParsedTokenTypes.Text, new TextTokenContent{Text = "666"}),
                        //new ParsedToken(ParsedTokenTypes.Normal, Encoding.UTF8.GetBytes("233"))
                    ],
                    "$$"
                ),
            UserCommandSource.Friend("", "", 0)
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
        CommandManager.DisposeCommandInstance();
        ActionManager.DisposeAdapterInstance();
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
        //Remove Old Adapter and Plugin
        CommandManager.DisposeCommandInstance();
        ActionManager.DisposeAdapterInstance();
        HttpRequest.SetConfig(Configuration["HttpRequest:TimeoutMS"],Configuration["HttpRequest:MaxBufferSizeByte"]);
        //Load Adapters
        List<IAdapterInstance> adapterList = AdapterLoader.LoadAdapters();
        //Load Plugins
        List<IPluginInstance> pluginList = PluginLoader.LoadPlugins();
        string pluginListText = JsonSerializer.Serialize(pluginList, _serializerOptions);
        string adapterListText = JsonSerializer.Serialize(adapterList, _serializerOptions);
        string commandReferenceText = JsonSerializer.Serialize(
            CommandManager.CommandInstanceIndexByAdapter.ToDictionary(
                k => k.Key,
                v => v.Value.Select(x =>
                    x.ExportToCommandProperties(ActionManager.AdapterInstanceReference)).ToArray()),
            _serializerOptions);

        FileIO.WriteFile(Path.Join(_programRoot, "loaded_adapters.json"), adapterListText);

        FileIO.WriteFile(Path.Join(_programRoot, "loaded_plugins.json"), pluginListText);

        Logger.LogInformation("Loaded Adapters:{AdapterList}", adapterListText);

        Logger.LogInformation("Loaded Plugins:{PluginList}", pluginListText);

        Logger.LogInformation("Loaded Commands:{CommandList}", commandReferenceText);
    }
}