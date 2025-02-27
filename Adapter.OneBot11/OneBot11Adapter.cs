using System.Text.Json;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Adapter.ActionParam;

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
        HttpServer hs = new(AdapterConfig, SubmitCommandEventAsync,Logger);
        MainLoopInstance = hs.ExecuteAsync(Cts.Token);
    }

    public async override Task<byte[]?> HandleCustomActionAsync(string action, CustomActionParameterWrapper? paras)
    {
        //None
        return null;
    }

    public override async Task<byte[]?> HandleDefaultActionAsync(DefaultActionType action, DefaultActionParameterWrapper? paras)
    {
        Console.WriteLine(paras?.Parameter?.GetType());

        switch (paras?.Parameter)
        {
            case SendGroupMgsParam sgmp:
                Console.WriteLine(JsonSerializer.Serialize(sgmp));
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
    }
}