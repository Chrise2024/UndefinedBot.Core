using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Plugin;

public interface IPluginInstance : IDisposable
{
    string Id { get; }
    string Name { get; }
    string[] TargetAdapter { get; }
    long[] GroupId { get; }
    internal List<CommandInstance> GetCommandInstance();
    void Initialize();
}

public abstract class BasePlugin : IPluginInstance
{
    /// <summary>
    /// Plugin's identifier,must be unique
    /// </summary>
    public abstract string Id { get; }

    public abstract string Name { get; }

    /// <summary>
    /// Adapter's identifier that plugin will docker on
    /// </summary>
    public abstract string[] TargetAdapter { get; }

    public long[] GroupId { get; }
    public abstract void Initialize();
    protected ILogger Logger { get; }
    protected HttpRequest Request => new(Logger.Extend("HttpRequest"));
    protected PluginConfigData PluginConfig { get; }
    protected string PluginPath => Path.GetDirectoryName(GetType().Assembly.Location) ?? "/";
    private List<CommandInstance> CommandInstances { get; } = [];

    List<CommandInstance> IPluginInstance.GetCommandInstance()
    {
        return CommandInstances;
    }

    protected BasePlugin()
    {
        PluginConfig = GetPluginConfig();
        GroupId = PluginConfig.GroupId;
        Logger = Shared.LoggerFactory.CreateCategoryLogger(GetType());
    }

    private PluginConfigData GetPluginConfig()
    {
        JsonNode originJson = FileIO.ReadAsJson(Path.Join(PluginPath, "plugin.json")) ??
                              throw new PluginLoadFailedException("Config File Not Exist");
        PluginConfigData? pluginConfigData = originJson.Deserialize<PluginConfigData>();
        if (pluginConfigData is null || !pluginConfigData.IsValid())
            throw new PluginLoadFailedException("Invalid config file");

        pluginConfigData.Implement(originJson);
        return pluginConfigData;
    }

    /// <summary>
    /// Register Command
    /// </summary>
    /// <param name="commandName">
    /// Command Name to be Called
    /// </param>
    /// <returns>
    /// <see cref="CommandInstance"/>
    /// </returns>
    protected CommandInstance RegisterCommand(string commandName)
    {
        Logger.Info($"Command {commandName} registered");
        CommandInstance ci = new(commandName, Id, TargetAdapter);
        CommandInstances.Add(ci);
        return ci;
    }

    public virtual void Dispose()
    {
        Array.Clear(GroupId);
        foreach (CommandInstance ci in CommandInstances) ci.Dispose();

        CommandInstances.Clear();
        Request.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// This Class records what write in config file
/// </summary>
[Serializable]
public sealed class PluginConfigData
{
    public string EntryFile { get; init; } = "";
    public string Description { get; init; } = "";
    public long[] GroupId { get; init; } = [];
    public JsonNode OriginalConfig { get; private set; } = JsonNode.Parse("{}")!;

    internal void Implement(JsonNode oc)
    {
        OriginalConfig = oc;
    }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(EntryFile);
    }
}

internal class PluginLoadFailedException(string? message) : Exception(message);