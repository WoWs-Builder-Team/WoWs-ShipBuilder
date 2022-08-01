using NLog;

namespace WoWsShipBuilder.Core;

public static class Logging
{
    public static Logger Logger { get; } = LogManager.GetLogger("ShipBuilder");

    public static Logger GetLogger(string name = "ShipBuilder") => LogManager.GetLogger(name);
}
