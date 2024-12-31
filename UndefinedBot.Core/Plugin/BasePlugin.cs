using System.Text.Json.Nodes;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Plugin;

public abstract class BasePlugin(PluginConfigData pluginConfig)
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string TargetAdapterId { get; }
    public abstract void Initialize();
    protected PluginLogger Logger => new(Name);
    protected UndefinedApi BaseApi => new(Name);
    protected PluginConfigData PluginConfig => pluginConfig;
    public List<CommandInstance> CommandInstances { get; } = [];
    /// <summary>
    /// Register Command
    /// </summary>
    /// <param name="commandName">
    /// Command Name to be Called
    /// </param>
    /// <returns>
    /// CommandInstance
    /// </returns>
    protected CommandInstance RegisterCommand(string commandName)
    {
        CommandInstance ci = new(commandName,TargetAdapterId);
        CommandInstances.Add(ci);
        return ci;
    }
}

/// <summary>
/// This Class records what write in config file
/// </summary>
[Serializable] public sealed class PluginConfigData
{
    public string EntryFile { get; init; } = "";
    public string Description { get; init; } = "";
    public List<long> GroupIds { get; init; } = [];
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
/// <summary>
/// This class is for program record plugin's properties,include information defined in assembly
/// </summary>
[Serializable] public sealed class PluginProperties(BasePlugin baseInstance,PluginConfigData originData)
{
    public string Id => baseInstance.Id;
    public string Name => baseInstance.Name;
    public string TargetAdapterId => baseInstance.TargetAdapterId;
    public List<long> GroupIds => originData.GroupIds;
    public string Description => originData.Description;
}
