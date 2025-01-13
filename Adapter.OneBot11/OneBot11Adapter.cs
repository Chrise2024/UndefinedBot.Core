﻿using System.Text.Json;
using UndefinedBot.Core.Adapter;

namespace Adapter.OneBot11;

public sealed class OneBot11Adapter : BaseAdapter
{
    public override string Id => "OneBot11Adapter";
    public override string Name => "OneBot11Adapter";
    public override string Platform => "QQ";
    public override string Protocol => "OneBot11";
    private Task MainLoopInstance { get; set; }
    private HttpApi HApi => new(AdapterConfig,Logger);
    private CancellationTokenSource Cts { get; } = new();
    public OneBot11Adapter()
    {
        HttpServer hs = new(AdapterConfig, SubmitCommandEvent,Logger);
        MainLoopInstance = hs.ExecuteAsync(Cts.Token);
    }

    public override byte[]? HandleCustomAction(string action, byte[]? paras)
    {
        //None
        return null;
    }

    public override byte[]? HandleDefaultAction(DefaultActionType action, object? paras)
    {
        Console.WriteLine(JsonSerializer.Serialize(paras));
        //None
        return null;
    }

    public override void Dispose()
    {
        Cts.Cancel();
        Cts.Dispose();
        MainLoopInstance.Dispose();
    }
}