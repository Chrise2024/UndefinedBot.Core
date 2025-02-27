using UndefinedBot.Core.Adapter.ActionParam;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Utils.Logging;

namespace UndefinedBot.Core.Adapter;

public sealed class ActionManager(CommandInformation cip, CommandLogger logger) : IDisposable
{
    internal static Dictionary<string, IAdapterInstance> AdapterInstanceReference { get; private set; } = [];
    private CommandInformation Information => cip;
    private CommandLogger Logger => logger;
    public async Task<byte[]?> InvokeDefaultAction(DefaultActionType action, DefaultActionParameterWrapper? paras = null)
    {
        try
        {
            return AdapterInstanceReference.TryGetValue(Information.AdapterId, out IAdapterInstance? inst)
                ? await inst.HandleDefaultActionAsync(action, paras)
                : null;
        }
        catch (Exception ex)
        {
            Logger.Error(ex,"Fail to Invoke Action");
        }

        return null;
    }
    public async Task<byte[]?> InvokeCustomAction(string action, CustomActionParameterWrapper? paras = null)
    {
        try
        {
            return AdapterInstanceReference.TryGetValue(Information.AdapterId, out IAdapterInstance? inst)
                ? await inst.HandleCustomActionAsync(action, paras)
                : null;
        }
        catch (Exception ex)
        {
            Logger.Error(ex,"Fail to Invoke Action");
        }

        return null;
    }

    internal static void UpdateAdapterInstances(IEnumerable<IAdapterInstance> ir)
    {
        AdapterInstanceReference.Clear();
        AdapterInstanceReference = ir.ToDictionary(k => k.Id,v => v);
        GC.Collect();
    }
    public void Dispose()
    {
        AdapterInstanceReference.Clear();
        Logger.Dispose();
        Information.Dispose();
    }

    internal static void DisposeAdapterInstance()
    {
        foreach (var pair in AdapterInstanceReference)
        {
            pair.Value.Dispose();
        }
    }
}

public enum DefaultActionType
{
    SendPrivateMsg = 0,
    SendGroupMsg = 1,
    RecallMessage = 2,
    GetMessage = 3,
    GetGroupMemberInfo = 4,
    GetGroupMemberList = 5,
    GroupMute = 6,
    GroupKick = 7,
}

public class DefaultActionParameterWrapper
{
    public readonly string Target;
    public readonly IDefaultActionParam? Parameter;
    private DefaultActionParameterWrapper(string target, IDefaultActionParam? parameter = default)
    {
        Target = target;
        Parameter = parameter;
    }
    public static DefaultActionParameterWrapper Direct(IDefaultActionParam content)
    {
        return new DefaultActionParameterWrapper("", content);
    }
    public static DefaultActionParameterWrapper TargetOnly(string target)
    {
        return new DefaultActionParameterWrapper(target);
    }
    public static DefaultActionParameterWrapper Common(string target, IDefaultActionParam content)
    {
        return new DefaultActionParameterWrapper(target, content);
    }
}
public class CustomActionParameterWrapper
{
    public readonly string Target;
    public readonly byte[]? Parameter;
    private CustomActionParameterWrapper(string target, byte[]? parameter = null)
    {
        Target = target;
        Parameter = parameter;
    }
    public static CustomActionParameterWrapper Direct(byte[] content)
    {
        return new CustomActionParameterWrapper("", content);
    }
    public static CustomActionParameterWrapper TargetOnly(string target)
    {
        return new CustomActionParameterWrapper(target);
    }
    public static CustomActionParameterWrapper Common(string target, byte[] content)
    {
        return new CustomActionParameterWrapper(target, content);
    }
}