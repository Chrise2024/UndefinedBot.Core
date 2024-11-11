using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace UndefinedBot.Core.Utils
{
    public class ConfigManager
    {
        private readonly JSchema _configJsonSchema = JSchema.Parse(
            @"
            {
            ""type"": ""object"",
            ""properties"": {
                    ""http_server_url"": { ""type"": ""string"" , ""format"": ""uri"" },
                    ""http_post_url"": { ""type"": ""string"" , ""format"": ""uri"" },
                    ""group_id"": { ""type"": ""array"", ""items"": { ""type"": ""integer"" } },
                    ""command_prefix"": { ""type"": ""string"" }
                },
                ""required"": [""http_server_url"", ""http_post_url"", ""group_id"", ""command_prefix""]
            }"
            );

        private readonly string _configPath = Path.Join(Core.GetCoreRoot(), "config.json");

        private Config _config;

        private readonly Config _defaultConfig = new("http://127.0.0.1:8087/", "http://127.0.0.1:8085", [], "#");

        /*
         * 8087为Bot上报消息的Url，即当前程序开启的Http Server地址
         * 8085为Bot接收Http请求的Url，即当前程序发送Http请求的地址
         */

        public ConfigManager()
        {
            if (!File.Exists(_configPath))
            {
                FileIO.WriteAsJson<Config>(_configPath, _defaultConfig);
                _config = _defaultConfig;
            }
            else
            {
                JObject RConfig = FileIO.ReadAsJson(_configPath);
                if (RConfig.IsValid(_configJsonSchema))
                {
                    _config = RConfig.ToObject<Config>();
                }
                else
                {
                    _config = _defaultConfig;
                    FileIO.WriteAsJson<Config>(_configPath, _defaultConfig);
                }
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
    }
    public struct Config(
        string httpServerUrl,
        string httpPostUrl,
        List<long> groupId,
        string commandPrefix
        )
    {
        [JsonProperty("http_server_url")] public string HttpServerUrl = httpServerUrl;
        [JsonProperty("http_post_url")] public string HttpPostUrl = httpPostUrl;
        [JsonProperty("group_id")] public List<long> GroupId = groupId;
        [JsonProperty("command_prefix")] public string CommandPrefix = commandPrefix;
    }
}