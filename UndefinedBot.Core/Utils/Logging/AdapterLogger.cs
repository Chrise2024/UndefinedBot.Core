namespace UndefinedBot.Core.Utils.Logging;

public sealed class AdapterLogger : BaseLogger
{
    protected override string Template { get; }
    protected override string[] Tags { get; }

    private readonly string _adapterName;
    
    private AdapterLogger(string adapterName,IEnumerable<string> tags)
    {
        _adapterName = adapterName;
        Tags = tags.ToArray();
        string extendTemplate = "";
        for (int i = 0; i < Tags.Length; i++)
        {
            extendTemplate += $"[{{Tag{i}}}] ";
        }

        Template = "[{Time}] [Adapter] " + $"[{_adapterName}] {extendTemplate}" + "[{LogLevel}] {Message}";
    }

    internal AdapterLogger(string adapterName)
    {
        _adapterName = adapterName;
        Tags = [];
        Template = "[{Time}] [Adapter] " + $"[{_adapterName}] "+ "[{LogLevel}] {Message}";
    }
    
    public AdapterLogger Extend(string subSpace)
    {
        return new AdapterLogger(_adapterName,[..Tags, subSpace]);
    }
    public AdapterLogger Extend(IEnumerable<string> subSpace)
    {
        return new AdapterLogger(_adapterName,[..Tags, ..subSpace]);
    }
}