using UndefinedBot.Core.Utils;

namespace UndefinedBot.Net.Utils.Logging;

public sealed class PluginLogger : BaseLogger
{
    protected override string Template { get; }
    protected override string[] Tags { get; } = [];
    public string PluginName { get; internal set; }
    protected override Microsoft.Extensions.Logging.ILogger<PluginLogger> RootLogger { get; }

    private PluginLogger(Microsoft.Extensions.Logging.ILogger<PluginLogger> rootLogger, string pluginName,
        IEnumerable<string> subSpace)
    {
        PluginName = pluginName;
        RootLogger = rootLogger;
        Tags = subSpace.ToArray();
        string extendTemplate = "";
        for (int i = 0; i < Tags.Length; i++) extendTemplate += $"[{{Tag{i}}}] ";
        Template = "[{Time}] [Plugin] " + $"[{pluginName}] {extendTemplate} " + "[{LogLevel}] {Message}";
    }

    internal PluginLogger(Microsoft.Extensions.Logging.ILogger<PluginLogger> rootLogger, string pluginName)
    {
        PluginName = pluginName;
        RootLogger = rootLogger;
        Template = "[{Time}] [Plugin] " + $"[{pluginName}] " + "[{LogLevel}] {Message}";
    }

    public override ILogger Extend(string subSpace)
    {
        return new PluginLogger(RootLogger, PluginName, [subSpace]);
    }

    public override ILogger Extend(IEnumerable<string> subSpace)
    {
        return new PluginLogger(RootLogger, PluginName, subSpace);
    }
}