using System.Text.Json.Nodes;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Plugin;

public interface IPluginInstance
{
    string Id { get; }
    string Name { get; }
    string TargetAdapterId { get; }
    List<long> GroupId { get; }
    internal List<CommandInstance> GetCommandInstance();
    void Initialize();
}

public abstract class BasePlugin(PluginConfigData pluginConfig) : IPluginInstance
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string TargetAdapterId { get; }
    public List<long> GroupId => pluginConfig.GroupId;
    public abstract void Initialize();
    protected ILogger Logger => new PluginLogger(Name);
    protected UndefinedApi BaseApi => new(Name);
    protected PluginConfigData PluginConfig => pluginConfig;
    private List<CommandInstance> CommandInstances { get; } = [];
    List<CommandInstance> IPluginInstance.GetCommandInstance() => CommandInstances;
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
        CommandInstance ci = new(commandName,Id,TargetAdapterId);
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
    public List<long> GroupId { get; init; } = [];
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
[Obsolete("These Properties Is Already Included in IPluginInstance",true)]
[Serializable] public sealed class PluginProperties(BasePlugin baseInstance,PluginConfigData originData)
{
    public string Id => baseInstance.Id;
    public string Name => baseInstance.Name;
    public string TargetAdapterId => baseInstance.TargetAdapterId;
    public List<long> GroupId => originData.GroupId;
    public string Description => originData.Description;
}
