﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UndefinedBot.Core.Command;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Adapter;

public sealed class ActionInvokeManager(CommandInvokeProperties cip, ILogger logger)
{
    private static Dictionary<string, IAdapterInstance> AdapterInstanceReference { get; set; } = [];
    private CommandInvokeProperties InvokeProperties => cip;
    private ILogger Logger => logger;

    public byte[]? InvokeDefaultAction(DefaultActionType action, byte[]? paras = null)
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

    internal static void UpdateAdapterInstances(Dictionary<string, IAdapterInstance> ir)
    {
        AdapterInstanceReference = ir;
    }
}