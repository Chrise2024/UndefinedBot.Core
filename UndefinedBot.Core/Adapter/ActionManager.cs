// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UndefinedBot.Core.Command;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Adapter;

public sealed class ActionManager(CommandInvokeProperties cip,CommandLogger logger)
{
    private static Dictionary<string, AdapterInstance> AdapterInstances { get; set; } = [];
    private CommandInvokeProperties InvokeProperties => cip;
    private CommandLogger Logger => logger;
    public byte[]? InvokeDefaultAction(DefaultActionType action, byte[]? paras = null)
    {
        try
        {
            return AdapterInstances.TryGetValue(InvokeProperties.AdapterId, out AdapterInstance? inst)
                ? inst.InvokeAction(action, paras)
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
            return AdapterInstances.TryGetValue(InvokeProperties.AdapterId, out AdapterInstance? inst)
                ? inst.InvokeAction(action, paras)
                : null;
        }
        catch (Exception)
        {
            Logger.Error("Fail to Invoke Action");
        }

        return null;
    }

    internal static void UpdateAdapterInstances(Dictionary<string, AdapterInstance> ir)
    {
        AdapterInstances = ir;
    }
}
