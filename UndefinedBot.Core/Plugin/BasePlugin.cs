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
[Serializable] public class PluginConfigData : IEquatable<PluginConfigData>
{
    public string EntryFile { get; set; } = "";
    public string Description { get; set; } = "";
    public List<long> GroupIds { get; set; } = [];
    public JsonNode OriginalConfig { get; set; } = JsonNode.Parse("{}")!;

    public bool Equals(PluginConfigData? other)
    {
        return EntryFile == other?.EntryFile &&
               Description == other.Description;
    }
}
/// <summary>
/// This class is for program record plugin's properties,include information defined in assembly
/// </summary>
[Serializable] public class PluginProperties(BasePlugin baseInstance,PluginConfigData originData)
{
    public string Id => baseInstance.Id;
    public string Name => baseInstance.Name;
    public string TargetAdapterId => baseInstance.TargetAdapterId;
    public List<long> GroupIds => originData.GroupIds;
    public string Description => originData.Description;
}
