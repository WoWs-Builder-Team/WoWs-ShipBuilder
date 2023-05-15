using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using NullLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger;

namespace WoWsShipBuilder.Common.Infrastructure;

public static class Logging
{
    public static ILogger Logger { get; private set; } = NullLogger.Instance;

    public static ILoggerFactory LoggerFactory { get; private set; } = NullLoggerFactory.Instance;

    public static void Initialize(ILoggerFactory loggerFactory)
    {
        LoggerFactory = loggerFactory;
        Logger = LoggerFactory.CreateLogger("WoWsShipBuilder");
    }
}
