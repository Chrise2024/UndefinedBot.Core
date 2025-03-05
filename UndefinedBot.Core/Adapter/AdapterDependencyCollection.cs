using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Adapter;

public readonly struct AdapterDependencyCollection
{
    public required IReadonlyConfig AdapterConfig { get; init; }
    public required ILoggerFactory LoggerFactory { get; init; }
}