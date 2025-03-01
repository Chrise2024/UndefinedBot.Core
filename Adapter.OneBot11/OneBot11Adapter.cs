using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Adapter.ActionParam;
using UndefinedBot.Core.Plugin.BasicMessage;

namespace Adapter.OneBot11;

public sealed class OneBot11Adapter : BaseAdapter
{
    public override string Id => "OneBot11Adapter";
    public override string Name => "OneBot11Adapter";
    public override string Platform => "QQ";
    public override string Protocol => "OneBot11";
    [AllowNull]private Task MainLoopInstance { get; set; }
    private HttpApi HApi => new(AdapterConfig,Logger);
    private CancellationTokenSource Cts { get; } = new();
    
    public override void Initialize()
    {
        HttpServer hs = new(AdapterConfig, SubmitCommandEvent,Logger);
        MainLoopInstance = hs.ExecuteAsync(Cts.Token);
    }

    public override async Task<byte[]?> HandleActionAsync(ActionType action, string? target = null,IActionParam? parameter = null)
    {
        Console.WriteLine(parameter?.GetType());

        switch (parameter)
        {
            case SendGroupMgsParam sgmp:
                Console.WriteLine(JsonSerializer.Serialize(((TextMessageNode)sgmp.MessageChain[0]).Text));
                break;
            case SendPrivateMsgParam spmp:
                Console.WriteLine(spmp);
                break;
        }
        //None
        return null;
    }

    public override void Dispose()
    {
        Cts.Cancel();
        Cts.Dispose();
        MainLoopInstance.Dispose();
        base.Dispose();
    }
}