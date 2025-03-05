using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Plugin;

public readonly struct PluginDependencyCollection
{
    public required IReadonlyConfig PluginConfig { get; init; }
    public required ILoggerFactory LoggerFactory { get; init; }
}