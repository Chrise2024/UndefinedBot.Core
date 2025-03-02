using Microsoft.Extensions.DependencyInjection;
using UndefinedBot.Core.Utils;
using MsILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using ILoggerFactory = UndefinedBot.Core.Utils.ILoggerFactory;

namespace UndefinedBot.Net.Utils.Logging;

public sealed class LoggerFactory(IServiceProvider provider) : ILoggerFactory
{
    public ILogger CreateCategoryLogger<TCategoryName>() where TCategoryName : notnull
    {
        return ExtendableLogger.Create<TCategoryName>(
            provider.GetRequiredService<MsILoggerFactory>()
        );
    }

    public ILogger CreateCategoryLogger<TCategoryName>(string[] tags) where TCategoryName : notnull
    {
        return ExtendableLogger.Create<TCategoryName>(
            provider.GetRequiredService<MsILoggerFactory>(), tags
        );
    }

    public ILogger CreateCategoryLogger(Type type)
    {
        return ExtendableLogger.Create(provider.GetRequiredService<MsILoggerFactory>(), type);
    }

    public ILogger CreateCategoryLogger(Type type, string[] tags)
    {
        return ExtendableLogger.Create(provider.GetRequiredService<MsILoggerFactory>(), tags, type);
    }
}