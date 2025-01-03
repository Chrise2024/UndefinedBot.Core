
namespace UndefinedBot.Core.Utils;

public static class ConfigManager
{
    private static Config _configData = new();

    //private static Config InitConfig(Config initData) => initData;
    // {
    //     return FileIO.ReadAsJson<Config>(_configPath)!;
    // }
    public static void InitConfig(Config initData)
    {
        _configData = initData;
    }

    public static List<long> GetGroupList()
    {
        return _configData.GroupId;
    }

    public static string GetCommandPrefix()
    {
        return _configData.CommandPrefix;
    }

    public static Config GetConfig()
    {
        return _configData;
    }
}
[Serializable] public sealed class Config
{
    public List<long> GroupId { get; set; } = [];
    public string CommandPrefix { get; set; } = "!";

}
