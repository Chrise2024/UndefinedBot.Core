using System.Reflection;
using System.Runtime.CompilerServices;
using UndefinedBot.Core.NetWork;

[assembly: InternalsVisibleTo("UndefinedBot.Net")]
[assembly: InternalsVisibleTo("UndefinedBot.Core.Test")]

namespace UndefinedBot.Core;

public delegate void CommandFinishHandler();

//public delegate Task CommandActionHandler(CommandContext ctx);

public sealed class CommandFinishEvent
{
    public event CommandFinishHandler? OnCommandFinish;

    public void Trigger()
    {
        OnCommandFinish?.Invoke();
    }
}
[Obsolete("This Api will be merge into BasePlugin",true)]
public sealed class UndefinedApi(string pluginName)
{
    public string RootPath => Environment.CurrentDirectory;

    public string PluginPath => Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) ??
                                throw new DllNotFoundException("Get Plugin Assembly Failed");

    public string CachePath => Path.Join(RootPath, "Cache", pluginName);
    public HttpRequest Request => new(pluginName);
    //public static RootConfigData MainRootConfigDataData => RootConfigManager.GetConfig();
}

public sealed class ProgramEnvironmentIfo
{
    public static ProgramEnvironmentIfo Instance => new();
    public string ProgramRootPath => Environment.CurrentDirectory;
    public PlatformID PlatForm => Environment.OSVersion.Platform;
    public bool Is64Bit => Environment.Is64BitOperatingSystem;
}