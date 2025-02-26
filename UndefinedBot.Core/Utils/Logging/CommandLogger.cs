namespace UndefinedBot.Core.Utils.Logging;

public sealed class CommandLogger : BaseLogger
{
    protected override string Template { get; }
    protected override string[] Tags { get; }
    
    private readonly string _pluginName;
    
    private readonly string _commandName;
    
    internal CommandLogger(string pluginName, string commandName,IEnumerable<string> tags)
    {
        _pluginName = pluginName;
        _commandName = commandName;
        Tags = tags.ToArray();
        string extendTemplate = "";
        for (int i = 0; i < Tags.Length; i++)
        {
            extendTemplate += $"[{{Tag{i}}}] ";
        }

        Template = "[{Time}] [Command] " + $"[{_pluginName}/{_commandName}] {extendTemplate} " + "[{LogLevel}] {Message}";
    }
    
    internal CommandLogger(string pluginName, string commandName)
    {
        _pluginName = pluginName;
        _commandName = commandName;
        Tags = [];
        Template = "[{Time}] [Command] " + $"[{_pluginName}/{_commandName}]" + "[{LogLevel}] {Message}";
    }
    
    public CommandLogger Extend(string subSpace)
    {
        return new CommandLogger(_pluginName,_commandName,[..Tags, subSpace]);
    }
    public CommandLogger Extend(IEnumerable<string> subSpace)
    {
        return new CommandLogger(_pluginName,_commandName,[..Tags, ..subSpace]);
    }
}