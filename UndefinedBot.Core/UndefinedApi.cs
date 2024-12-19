using System.Reflection;
using System.Runtime.CompilerServices;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Utils;

[assembly:InternalsVisibleTo("UndefinedBot.Net")]
[assembly:InternalsVisibleTo("UndefinedBot.Core.Test")]
namespace UndefinedBot.Core;

public delegate void CommandFinishHandler();
public delegate Task CommandActionHandler(CommandContext ctx);
public class CommandFinishEvent
{
    public event CommandFinishHandler? OnCommandFinish;

    public void Trigger()
    {
        OnCommandFinish?.Invoke();
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class PluginAttribute : Attribute;
public class UndefinedApi(string pluginName)
{
    public  string PluginName => pluginName;
    public string RootPath => Environment.CurrentDirectory;
    public string PluginPath => Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) ?? throw new DllNotFoundException("Get Plugin Assembly Failed");
    public string CachePath => Path.Join(RootPath, "Cache", pluginName);
    public GeneralLogger Logger => new(pluginName);
    //public HttpApi Api => new("");//ConfigData.GetHttpPostUrl());
    public HttpRequest Request => new(pluginName);
    public CommandFinishEvent FinishEvent => new();
    public CacheManager Cache => new(pluginName, CachePath, FinishEvent);
    public static Config MainConfigData => ConfigManager.GetConfig();
    internal readonly List<CommandInstance> _commandInstances = [];
    /// <summary>
    /// Register Command
    /// </summary>
    /// <param name="commandName">
    /// Command Name to be Called
    /// </param>
    /// <returns>
    /// CommandInstance
    /// </returns>
    public CommandInstance RegisterCommand(string commandName)
    {
        CommandInstance ci = new(commandName);
        _commandInstances.Add(ci);
        return ci;
    }
}
