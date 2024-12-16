using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UndefinedBot.Net.NetWork;

public class NetworkServiceCollection(IServiceProvider services,
    IConfiguration config,
    ILogger<NetworkServiceCollection> logger
    ) : IHostedService
{
    private readonly List<(IServiceScope, HttpServer)> _networkServices = [];
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var scope = services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        HttpServer hs = new HttpServer(services.GetRequiredService<ILogger<HttpServer>>());
        try
        {
            await hs.StartAsync(cancellationToken);
            _networkServices.Add((scope,hs));
        }
        catch (Exception ex)
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
            catch (Exception e)
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
