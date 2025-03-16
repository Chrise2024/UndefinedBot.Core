using UndefinedBot.Core.Command;
using UndefinedBot.Core.MessageProcessor;
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
    protected ILogger Logger { get; }
    protected HttpRequest Request => new(Logger.Extend("HttpRequest"));
    protected IReadonlyConfig PluginConfig { get; }
    protected string PluginPath => Path.GetDirectoryName(GetType().Assembly.Location) ?? "/";
    private List<CommandInstance> CommandInstances { get; } = [];
    
    public abstract void Initialize();

    List<CommandInstance> IPluginInstance.GetCommandInstance()
    {
        return CommandInstances;
    }

    protected BasePlugin(PluginDependencyCollection dependencyCollection)
    {
        Logger = dependencyCollection.LoggerFactory.CreateCategoryLogger(GetType());
        PluginConfig = dependencyCollection.PluginConfig;
        GroupId = PluginConfig.GetValue<long[]>("GroupId") ?? throw new Exception("GroupId not found");
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
        CommandInstance ci = new(commandName, Id, TargetAdapter,Logger);
        CommandInstances.Add(ci);
        return ci;
    }
    
    protected MessageProcessorInstance RegisterMessageProcessor(string filterName)
    {
        MessageProcessorInstance mfi = new(filterName, Id, Logger);
        return mfi;
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

internal class PluginLoadFailedException(string? message) : Exception(message);