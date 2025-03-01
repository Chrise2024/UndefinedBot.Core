using System.Text;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using UndefinedBot.Core.Utils;
using System.Runtime;
using Microsoft.Extensions.DependencyInjection;
using UndefinedBot.Net.Utils;

namespace UndefinedBot.Net;

internal class Program
{
    private static readonly string _programRoot = Environment.CurrentDirectory;

    private static readonly string _programCache = Path.Join(_programRoot, "Cache");

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        GCSettings.LatencyMode = GCLatencyMode.Batch;
        FileIO.EnsurePath(_programCache);
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
        undefinedAppBuilder.Services.AddHostedService<LogService>();
        UndefinedApp undefinedApp = new(undefinedAppBuilder.Build());
        undefinedApp.Start();
        
        await undefinedApp.StopAsync();
    }

    public static string GetProgramRoot()
    {
        return _programRoot;
    }

    public static string GetProgramCache()
    {
        return _programCache;
    }
}