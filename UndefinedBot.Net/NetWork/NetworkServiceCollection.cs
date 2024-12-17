using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UndefinedBot.Net.NetWork;

public class NetworkServiceCollection(
    IServiceProvider services,
    ILogger<NetworkServiceCollection> logger
    ) : IHostedService
{
    private readonly List<(IServiceScope, HttpServer)> _networkServices = [];
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var scope = services.CreateScope();
        HttpServer hs = new (services.GetRequiredService<ILogger<HttpServer>>());
        try
        {
            await hs.StartAsync(cancellationToken);
            _networkServices.Add((scope,hs));
        }
        catch (Exception)
        {
            logger.LogError("Network Service Start Failed");
            scope.Dispose();
        }
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var (scope, service) in _networkServices)
        {
            try
            {
                await service.StopAsync(cancellationToken);
            }
            catch (Exception)
            {
                logger.LogError("Network Service Stop Failed");
            }
            finally
            {
                scope.Dispose();
            }
        }
    }
}
