using System.Text.Json.Serialization;

namespace UndefinedBot.Core.Utils;

public class ConfigManager
{

    private readonly string _configPath = Path.Join(Environment.CurrentDirectory, "config.json");

    private readonly Config _config;

    private readonly Config _defaultConfig = new();

    /*
     * 8087为Bot上报消息的Url，即当前程序开启的Http Server地址
     * 8085为Bot接收Http请求的Url，即当前程序发送Http请求的地址
     */

    public ConfigManager()
    {
        if (File.Exists(_configPath))
        {
            Config rConfig = FileIO.ReadAsJson<Config>(_configPath);
            if (rConfig.Equals(default(Config)))
            {
                _config = _defaultConfig;
                FileIO.WriteAsJson(_configPath, _defaultConfig);
            }
            else
            {
                _config = rConfig;
            }
        }
        else
        {
            FileIO.WriteAsJson(_configPath, _defaultConfig);
            _config = _defaultConfig;
        }
    }

    public string GetHttpServerUrl()
    {
        return _config.HttpServerUrl;
    }

    public string GetHttpPostUrl()
    {
        return _config.HttpPostUrl;
    }

    public List<long> GetGroupList()
    {
        return _config.GroupId;
    }

    public string GetCommandPrefix()
    {
        return _config.CommandPrefix;
    }

    public Config GetConfig()
    {
        return _config;
    }
}

public readonly struct Config(
    string httpServerUrl = "",
    string httpPostUrl = "",
    List<long>? groupId = null,
    string commandPrefix = "!"
)
{
    [JsonPropertyName("http_server_url")] public readonly string HttpServerUrl = httpServerUrl;
    [JsonPropertyName("http_post_url")] public readonly string HttpPostUrl = httpPostUrl;
    [JsonPropertyName("group_id")] public readonly List<long> GroupId = groupId ?? [];
    [JsonPropertyName("command_prefix")] public readonly string CommandPrefix = commandPrefix;
}
