using UndefinedBot.Core.Utils;

namespace UndefinedBot.Net.Utils.Logging;

public sealed class CommandLogger : BaseLogger
{
    protected override string Template { get; }
    protected override string[] Tags { get; }
    
    private readonly string _pluginName;
    
    private readonly string _commandName;
    protected override Microsoft.Extensions.Logging.ILogger<CommandLogger> RootLogger { get; }
    
    internal CommandLogger(Microsoft.Extensions.Logging.ILogger<CommandLogger> rootLogger,string pluginName, string commandName,IEnumerable<string> tags)
    {
        _pluginName = pluginName;
        _commandName = commandName;
        RootLogger = rootLogger;
        Tags = tags.ToArray();
        string extendTemplate = "";
        for (int i = 0; i < Tags.Length; i++)
        {
            extendTemplate += $"[{{Tag{i}}}] ";
        }

        Template = "[{Time}] [Command] " + $"[{_pluginName}/{_commandName}] {extendTemplate} " + "[{LogLevel}] {Message}";
    }
    
    internal CommandLogger(Microsoft.Extensions.Logging.ILogger<CommandLogger> rootLogger,string pluginName, string commandName)
    {
        _pluginName = pluginName;
        _commandName = commandName;
        RootLogger = rootLogger;
        Tags = [];
        Template = "[{Time}] [Command] " + $"[{_pluginName}/{_commandName}]" + "[{LogLevel}] {Message}";
    }
    
    public override ILogger Extend(string subSpace)
    {
        return new CommandLogger(RootLogger,_pluginName,_commandName,[..Tags, subSpace]);
    }
    public override ILogger Extend(IEnumerable<string> subSpace)
    {
        return new CommandLogger(RootLogger,_pluginName,_commandName,[..Tags, ..subSpace]);
    }
}