using System.Text;
using System.Runtime;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UndefinedBot.Core.Utils;
using UndefinedBot.Net.Utils;
using MsILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using InternalILoggerFactory = UndefinedBot.Core.Utils.ILoggerFactory;
using MsLoggerFactory = Microsoft.Extensions.Logging.LoggerFactory;
using InternalLoggerFactory = UndefinedBot.Net.Utils.Logging.LoggerFactory;

namespace UndefinedBot.Net;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        GCSettings.LatencyMode = GCLatencyMode.Batch;
        FileIO.EnsurePath(Path.Join(Environment.CurrentDirectory, "Cache"));
        if (!File.Exists("appsettings.json"))
        {
            Console.WriteLine("No exist config file, create it now...");
            Stream resStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("UndefinedBot.Net.appsettings.json")!;
            FileStream tempStream = File.Create("appsettings.json");
            await resStream.CopyToAsync(tempStream);
            resStream.Close();
            tempStream.Close();
            Console.WriteLine("Please Edit the appsettings.json to set configs");
            return;
        }

        HostApplicationBuilder undefinedAppBuilder = new(args);
        undefinedAppBuilder.Configuration.AddJsonFile("appsettings.json", false, true);
        undefinedAppBuilder.Configuration.AddEnvironmentVariables();
        undefinedAppBuilder.Services.AddSingleton<AdapterLoadService>();
        undefinedAppBuilder.Services.AddSingleton<PluginLoadService>();
        undefinedAppBuilder.Services.AddSingleton<InternalILoggerFactory, InternalLoggerFactory>();
        undefinedAppBuilder.Services.AddSingleton<MsILoggerFactory, MsLoggerFactory>();
        UndefinedApp undefinedApp = new(undefinedAppBuilder.Build());
        undefinedApp.Start();

        await undefinedApp.StopAsync();
    }
}