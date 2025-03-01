using UndefinedBot.Core.Utils;

namespace UndefinedBot.Net.Utils.Logging;

public sealed class AdapterLogger : BaseLogger
{
    protected override string Template { get; }
    protected override string[] Tags { get; }

    private readonly string _adapterName;
    protected override Microsoft.Extensions.Logging.ILogger<AdapterLogger> RootLogger { get; }
    
    internal AdapterLogger(Microsoft.Extensions.Logging.ILogger<AdapterLogger> rootLogger,string adapterName,IEnumerable<string> tags)
    {
        _adapterName = adapterName;
        RootLogger = rootLogger;
        Tags = tags.ToArray();
        string extendTemplate = "";
        for (int i = 0; i < Tags.Length; i++)
        {
            extendTemplate += $"[{{Tag{i}}}] ";
        }

        Template = "[{Time}] [Adapter] " + $"[{_adapterName}] {extendTemplate}" + "[{LogLevel}] {Message}";
    }

    internal AdapterLogger(Microsoft.Extensions.Logging.ILogger<AdapterLogger> rootLogger,string adapterName)
    {
        _adapterName = adapterName;
        RootLogger = rootLogger;
        Tags = [];
        Template = "[{Time}] [Adapter] " + $"[{_adapterName}] "+ "[{LogLevel}] {Message}";
    }
    
    public override ILogger Extend(string subSpace)
    {
        return new AdapterLogger(RootLogger,_adapterName,[..Tags, subSpace]);
    }
    public override ILogger Extend(IEnumerable<string> subSpace)
    {
        return new AdapterLogger(RootLogger,_adapterName,[..Tags, ..subSpace]);
    }
}