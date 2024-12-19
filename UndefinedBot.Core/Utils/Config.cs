
namespace UndefinedBot.Core.Utils;

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
    public List<long> GroupId { get; set; } = [];
    public string CommandPrefix { get; set; } = "!";

}
