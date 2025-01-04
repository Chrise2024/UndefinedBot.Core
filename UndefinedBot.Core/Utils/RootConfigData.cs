namespace UndefinedBot.Core.Utils;

public static class RootConfigManager
{
    private static RootConfigData _rootConfigData = new();

    //private static Config InitConfig(Config initData) => initData;
    // {
    //     return FileIO.ReadAsJson<Config>(_configPath)!;
    // }
    public static void InitConfig(RootConfigData initData)
    {
        _rootConfigData = initData;
    }

    public static List<long> GetGroupList()
    {
        return _rootConfigData.GroupId;
    }

    public static string GetCommandPrefix()
    {
        return _rootConfigData.CommandPrefix;
    }

    public static RootConfigData GetConfig()
    {
        return _rootConfigData;
    }
}

[Serializable]
public sealed class RootConfigData
{
    public List<long> GroupId { get; set; } = [];
    public string CommandPrefix { get; set; } = "!";
}