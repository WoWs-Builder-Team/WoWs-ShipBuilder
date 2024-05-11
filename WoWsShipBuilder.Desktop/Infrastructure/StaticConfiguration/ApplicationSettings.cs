using System.Reflection;
using System.Text.Json;
using WoWsShipBuilder.Infrastructure.ApplicationData;

namespace WoWsShipBuilder.Desktop.Infrastructure.StaticConfiguration;

internal static class ApplicationSettings
{
    private static ApplicationOptions? instance;

    public static ApplicationOptions ApplicationOptions => instance ??= LoadOptions();

    private static ApplicationOptions LoadOptions()
    {
        var resourceName = "WoWsShipBuilder.Desktop.Infrastructure.StaticConfiguration.ApplicationOptions.json";
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        ApplicationOptions? result = null;
        if (stream != null)
        {
            result = JsonSerializer.Deserialize<ApplicationOptions>(stream, AppConstants.JsonSerializerOptions);
        }

        return result ?? new ApplicationOptions();
    }
}
