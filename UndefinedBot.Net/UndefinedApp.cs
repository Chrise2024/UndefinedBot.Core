using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.Arguments.TokenContentType;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Net.Utils;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Plugin;

namespace UndefinedBot.Net;

public sealed class UndefinedApp : IHost
{
    private readonly IHost _hostApp;
    public IServiceProvider Services => _hostApp.Services;
    
    private readonly ILogger<UndefinedApp> _logger;
    private readonly IConfiguration _configuration;
    private readonly AdapterService _adapterService;
    private readonly PluginService _pluginService;
    private readonly LogService _logService;

    private readonly string _programRoot = Environment.CurrentDirectory;

    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true, IndentSize = 4 };

    public UndefinedApp(IHost host)
    {
        _hostApp = host;
        _logger = Services.GetRequiredService<ILogger<UndefinedApp>>();
        _configuration = Services.GetRequiredService<IConfiguration>();
        _adapterService = Services.GetRequiredService<AdapterService>();
        _pluginService = Services.GetRequiredService<PluginService>();
        _logService = Services.GetRequiredService<LogService>();
    }

    public async Task StartAsync(CancellationToken cancellationToken = new())
    {
        _logService.StartLogging();
        //Load Adapter and Plugin
        Init();

        await _hostApp.StartAsync(cancellationToken);

        _logger.LogInformation("UndefinedBot.Net Implementation has started");
        //for test
        _ = await CommandManager.InvokeCommandAsync(
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
                ActionManager.DisposeAdapterInstance();
                _pluginService.Unload();
                Init();
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = new())
    {
        _pluginService.Unload();
        ActionManager.DisposeAdapterInstance();
        _logService.StopLogging();
        await _hostApp.StopAsync(cancellationToken);

        _logger.LogInformation("UndefinedBot.Net Implementation has stopped");
    }

    public void Dispose()
    {
        _adapterService.Dispose();
        _pluginService.Dispose();
        _logService.Dispose();
        _hostApp.Dispose();
    }

    private void Init()
    {
        HttpRequest.SetConfig(_configuration["HttpRequest:TimeoutMS"],_configuration["HttpRequest:MaxBufferSizeByte"]);
        //Load Adapters
        List<IAdapterInstance> adapterList = _adapterService.LoadAdapters();
        //Load Plugins
        List<IPluginInstance> pluginList = _pluginService.LoadPlugins();
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

        _logger.LogInformation("Loaded Adapters:{AdapterList}", adapterListText);

        _logger.LogInformation("Loaded Plugins:{PluginList}", pluginListText);

        _logger.LogInformation("Loaded Commands:{CommandList}", commandReferenceText);
    }
}