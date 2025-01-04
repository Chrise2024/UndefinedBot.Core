using System.Text;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Utils;
using System.Runtime;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Plugin;
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
        UndefinedApp undefinedApp = new(undefinedAppBuilder.Build());
        undefinedApp.Start();
        new GeneralLogger("1111").Info("6666666666");
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