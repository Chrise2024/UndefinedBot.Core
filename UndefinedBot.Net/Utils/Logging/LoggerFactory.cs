using Microsoft.Extensions.DependencyInjection;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Net.Utils.Logging;

public class LoggerFactory(IServiceProvider provider) : ILoggerFactory
{
    public ILogger CreateCategoryLogger<TCategoryName>() where TCategoryName : notnull
    {
        return ExtendableLogger.Create<TCategoryName>(
            provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>()
        );
    }

    public ILogger CreateCategoryLogger<TCategoryName>(string[] tags) where TCategoryName : notnull
    {
        return ExtendableLogger.Create<TCategoryName>(
            provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>(), tags
        );
    }
    public ILogger CreateCategoryLogger(Type type)
    {
        return ExtendableLogger.Create(provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>(), type);
    }

    public ILogger CreateCategoryLogger(Type type,string[] tags)
    {
        return ExtendableLogger.Create(provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>(), tags,type);
    }
}