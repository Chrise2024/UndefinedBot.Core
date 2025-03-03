using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.Arguments.TokenContentType;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Net.Utils;

namespace UndefinedBot.Net;

public sealed class UndefinedApp : IHost
{
    private readonly IHost _hostApp;
    public IServiceProvider Services => _hostApp.Services;

    private readonly ILogger<UndefinedApp> _logger;
    private readonly IConfiguration _configuration;
    private readonly AdapterLoadService _adapterLoadService;
    private readonly PluginLoadService _pluginLoadService;

    internal static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        IndentSize = 4,
    };

    public UndefinedApp(IHost host)
    {
        _hostApp = host;
        _logger = Services.GetRequiredService<ILogger<UndefinedApp>>();
        _configuration = Services.GetRequiredService<IConfiguration>();
        _adapterLoadService = Services.GetRequiredService<AdapterLoadService>();
        _pluginLoadService = Services.GetRequiredService<PluginLoadService>();
    }

    public async Task StartAsync(CancellationToken cancellationToken = new())
    {
        HttpRequest.SetConfig(
            _configuration["HttpRequest:TimeoutMS"],
            _configuration["HttpRequest:MaxBufferSizeByte"]
        );
        //Load Adapters
        _adapterLoadService.LoadAdapter();

        await _hostApp.StartAsync(cancellationToken);

        _logger.LogInformation("UndefinedBot.Net Implementation has started");
        //for test
        _logger.LogInformation("Test Command");
        _adapterLoadService.ExternalInvokeCommand(
            CommandInformation
                .Group("test", "0", "0", "0", 114514191)
                .Implement(
                    "OneBot11Adapter",
                    "",
                    "",
                    [
                        new ParsedToken(
                            ParsedTokenTypes.Text,
                            new TextTokenContent { Text = "123" }
                        ),
                        new ParsedToken(
                            ParsedTokenTypes.Text,
                            new TextTokenContent { Text = "456" }
                        ),
                        //new ParsedToken(ParsedTokenTypes.Normal, Encoding.UTF8.GetBytes("233"))
                    ],
                    "$$"
                ),
            UserCommandSource.Friend("", "", 0)
        );
        //Console Loop
        while (true)
        {
            string? tempString = Console.ReadLine();
            if (tempString == "stop")
                break;

            if (tempString == "reload")
            {
                _adapterLoadService.Unload();
                _pluginLoadService.Unload();
                HttpRequest.SetConfig(
                    _configuration["HttpRequest:TimeoutMS"],
                    _configuration["HttpRequest:MaxBufferSizeByte"]
                );
                //Load Plugins
                _pluginLoadService.LoadPlugin();
                //Load Adapters
                _adapterLoadService.LoadAdapter();
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = new())
    {
        _adapterLoadService.Unload();
        _pluginLoadService.Unload();
        await _hostApp.StopAsync(cancellationToken);
        _logger.LogInformation("UndefinedBot.Net Implementation has stopped");
    }

    public void Dispose()
    {
        _adapterLoadService.Dispose();
        _pluginLoadService.Dispose();
        _hostApp.Dispose();
    }
}
