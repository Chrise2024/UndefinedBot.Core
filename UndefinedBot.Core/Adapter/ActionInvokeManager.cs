using UndefinedBot.Core.Command;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Adapter;

public sealed class ActionInvokeManager(CommandInvokeProperties cip, ExtendableLogger logger) : IDisposable
{
    internal static Dictionary<string, IAdapterInstance> AdapterInstanceReference { get; set; } = [];
    private CommandInvokeProperties InvokeProperties => cip;
    private ExtendableLogger Logger => logger;
    public byte[]? InvokeDefaultAction(DefaultActionType action, object? paras = null)
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

    public byte[]? InvokeCustomAction(string action, byte[]? paras = null)
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