namespace UndefinedBot.Core.Command.CommandUtils;

internal sealed class CommandRateManager : IDisposable
{
    private TimeSpan RateLimit { get; set; } = TimeSpan.Zero;
    private Dictionary<string, DateTime> LastExecuteFriend { get; } = [];
    private Dictionary<string, DateTime> LastExecuteGroup { get; } = [];
    private Dictionary<string, DateTime> LastExecuteGuild { get; } = [];
    private DateTime LastExecuteGlobal { get; set; } = DateTime.MinValue;
    private CommandRateManagerMode Mode { get; set; } = CommandRateManagerMode.Disable;

    public void SetRateLimit(TimeSpan rateLimit)
    {
        RateLimit = rateLimit;
    }

    public void SetMode(CommandRateManagerMode mode)
    {
        Mode = mode;
    }

    public bool IsReachRateLimit(CommandInformation information)
    {
        return Mode switch
        {
            CommandRateManagerMode.Disable => false,
            CommandRateManagerMode.Individual => information.SubType switch
            {
                MessageSubType.Friend => IsReachFriendRateLimit(information.SourceId),
                MessageSubType.Group => IsReachGroupRateLimit(information.SourceId),
                MessageSubType.Guild => IsReachGuildRateLimit(information.SourceId),
                _ => false
            },
            _ => DateTime.Now - LastExecuteGlobal < RateLimit
        };
    }

    public void UpdateLastExecute(CommandInformation information)
    {
        DateTime nowTime = DateTime.Now;
        switch (Mode)
        {
            case CommandRateManagerMode.Disable:
                break;
            case CommandRateManagerMode.Individual:
                switch (information.SubType)
                {
                    case MessageSubType.Friend:
                        LastExecuteFriend[information.SourceId] = nowTime;
                        break;
                    case MessageSubType.Group:
                        LastExecuteGroup[information.SourceId] = nowTime;
                        break;
                    case MessageSubType.Guild:
                        LastExecuteGuild[information.SourceId] = nowTime;
                        break;
                }

                break;
        }

        LastExecuteGlobal = nowTime;
    }

    #region RateLimitJudgment

    private bool IsReachFriendRateLimit(string userId)
    {
        if (!LastExecuteFriend.TryGetValue(userId, out DateTime lastExecute)) return false;

        return DateTime.Now - lastExecute < RateLimit;
    }

    private bool IsReachGroupRateLimit(string groupId)
    {
        if (!LastExecuteGroup.TryGetValue(groupId, out DateTime lastExecute)) return false;

        return DateTime.Now - lastExecute < RateLimit;
    }

    private bool IsReachGuildRateLimit(string guildId)
    {
        if (!LastExecuteGuild.TryGetValue(guildId, out DateTime lastExecute)) return false;

        return DateTime.Now - lastExecute < RateLimit;
    }

    #endregion

    public void Dispose()
    {
        LastExecuteFriend.Clear();
        LastExecuteGroup.Clear();
        LastExecuteGuild.Clear();
    }
}

internal enum CommandRateManagerMode
{
    Global,
    Individual,
    Disable
}