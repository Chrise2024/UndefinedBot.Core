
namespace UndefinedBot.Core.Utils;

public class HttpServiceOptions
{
    public HttpServiceOptions(string host, string port, string? accessToken = null)
    {
        if (!uint.TryParse(port, out uint numberPort) || numberPort is <= 1023 or >= 65536)
        {
            throw new ArgumentException($"Invalid Port: {port}");
        }

        Host = host;
        Port = numberPort;
        AccessToken = accessToken;
    }
    public HttpServiceOptions(string host, uint port, string? accessToken = null)
    {
        if (port < 1023)
        {
            throw new ArgumentException($"Invalid Port: {port}");
        }
        Host = host;
        Port = port;
        AccessToken = accessToken;
    }
    public HttpServiceOptions()
    {
        Host = "";
        Port = 16384;
        AccessToken = null;
    }
    public string Host { get; }
    public uint Port { get; }
    public string? AccessToken { get; }
}

public abstract class ConfigManager
{
    private static Config s_configData = new();

    //private static Config InitConfig(Config initData) => initData;
    // {
    //     return FileIO.ReadAsJson<Config>(s_configPath)!;
    // }
    public static void InitConfig(Config initData)
    {
        s_configData = initData;
    }

    public static string GetHttpServerUrl() => $"http://{s_configData.HttpServer.Host}:{s_configData.HttpServer.Port}/";

    public static string GetHttpPostUrl() => $"http://{s_configData.HttpPost.Host}:{s_configData.HttpPost.Port}";

    public static List<long> GetGroupList()
    {
        return s_configData.GroupId;
    }

    public static string GetCommandPrefix()
    {
        return s_configData.CommandPrefix;
    }

    public static Config GetConfig()
    {
        return s_configData;
    }
}
[Serializable] public class Config
{
    public HttpServiceOptions HttpServer { get; set; } = new HttpServiceOptions("",16384);
    public HttpServiceOptions HttpPost { get; set; } = new HttpServiceOptions("",16384);
    public List<long> GroupId { get; set; } = [];
    public string CommandPrefix { get; set; } = "!";
    public string GetHttpServerUrl() => $"http://{HttpServer.Host}:{HttpServer.Port}/";
    public string GetHttpPostUrl() => $"http://{HttpPost.Host}:{HttpPost.Port}";

}
