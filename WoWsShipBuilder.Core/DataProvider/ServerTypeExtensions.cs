namespace WoWsShipBuilder.Core.DataProvider;

public static class ServerTypeExtensions
{
    /// <summary>
    /// Converts a <see cref="ServerType"/> value into a lowercase string that can be used in urls or file system paths.
    /// </summary>
    /// <param name="serverType">The <see cref="ServerType"/> to convert.</param>
    /// <returns>A string representation of the server type.</returns>
    public static string StringName(this ServerType serverType)
    {
        return serverType switch
        {
            ServerType.Live => "live",
            ServerType.Pts => "pts",
            ServerType.Dev1 => "dev1",
            ServerType.Dev2 => "dev2",
            ServerType.Dev3 => "dev3",
            _ => "none",
        };
    }

    /// <summary>
    /// Converts a <see cref="ServerType"/> value into a string that can be used in urls or file system paths.
    /// </summary>
    /// <param name="serverType">The <see cref="ServerType"/> to convert.</param>
    /// <returns>A string representation of the server type.</returns>
    public static string DisplayName(this ServerType serverType)
    {
        return serverType switch
        {
            ServerType.Live => "Live",
            ServerType.Pts => "PTS",
            ServerType.Dev1 => "Dev1",
            ServerType.Dev2 => "Dev2",
            ServerType.Dev3 => "Dev3",
            _ => "None",
        };
    }
}
