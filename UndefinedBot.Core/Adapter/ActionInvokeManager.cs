using UndefinedBot.Core.Adapter.ActionParam;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Adapter;

public sealed class ActionInvokeManager(CommandInvokeProperties cip, ExtendableLogger logger) : IDisposable
{
    internal static Dictionary<string, IAdapterInstance> AdapterInstanceReference { get; private set; } = [];
    private CommandInvokeProperties InvokeProperties => cip;
    private ExtendableLogger Logger => logger;
    public byte[]? InvokeDefaultAction(DefaultActionType action, DefaultActionParameterWrapper? paras = null)
    {
        try
        {
            return AdapterInstanceReference.TryGetValue(InvokeProperties.AdapterId, out IAdapterInstance? inst)
                ? inst.HandleDefaultAction(action, paras)
                : null;
        }
        catch (Exception)
        {
            Logger.Error("Fail to Invoke Action");
        }

        return null;
    }
    public byte[]? InvokeCustomAction(string action, CustomActionParameterWrapper? paras = null)
    {
        try
        {
            return AdapterInstanceReference.TryGetValue(InvokeProperties.AdapterId, out IAdapterInstance? inst)
                ? inst.HandleCustomAction(action, paras)
                : null;
        }
        catch (Exception)
        {
            Logger.Error("Fail to Invoke Action");
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
        InvokeProperties.Dispose();
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
    public string Target { get; init; }
    public IActionParam? Parameter { get; init; }
    private DefaultActionParameterWrapper(string target, IActionParam? parameter = default)
    {
        Target = target;
        Parameter = parameter;
    }
    public static DefaultActionParameterWrapper Direct(IActionParam content)
    {
        return new DefaultActionParameterWrapper("", content);
    }
    public static DefaultActionParameterWrapper TargetOnly(string target)
    {
        return new DefaultActionParameterWrapper(target);
    }
    public static DefaultActionParameterWrapper Common(string target, IActionParam content)
    {
        return new DefaultActionParameterWrapper(target, content);
    }
}
public class CustomActionParameterWrapper
{
    public string Target { get; init; }
    public byte[]? Parameter { get; init; }
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