using System.Diagnostics.CodeAnalysis;
using UndefinedBot.Core.Adapter.ActionParam;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Adapter;

public sealed class ActionInvokeManager(CommandInvokeProperties cip, ExtendableLogger logger) : IDisposable
{
    internal static Dictionary<string, IAdapterInstance> AdapterInstanceReference { get; set; } = [];
    private CommandInvokeProperties InvokeProperties => cip;
    private ExtendableLogger Logger => logger;
    public byte[]? InvokeDefaultAction<T>(DefaultActionType action, ActionContentWrapper<T>? paras = null) where T : IActionParam
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

    public byte[]? InvokeCustomAction(string action, ActionContentWrapper<CustomActionParam>? paras = null)
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
        Logger.Dispose();
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

public class ActionContentWrapper<T> where T : IActionParam
{
    public string Target { get; init; }
    public T? Content { get; init; }
    private ActionContentWrapper(string target, T? content = default)
    {
        Target = target;
        Content = content;
    }
    public static ActionContentWrapper<T> Direct(T content)
    {
        return new ActionContentWrapper<T>("", content);
    }
    public static ActionContentWrapper<T> TargetOnly(string target)
    {
        return new ActionContentWrapper<T>(target);
    }
    public static ActionContentWrapper<T> Common(string target, T content)
    {
        return new ActionContentWrapper<T>(target, content);
    }
}