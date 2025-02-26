namespace UndefinedBot.Core.Utils.Logging;

public sealed class PluginLogger : BaseLogger
{
    protected override string Template { get; }
    protected override string[] Tags { get; } = [];
    
    internal PluginLogger(string pluginName)
    {
        Template = "[{Time}] [Plugin] " + $"[{pluginName}] " + "[{LogLevel}] {Message}";
    }
}