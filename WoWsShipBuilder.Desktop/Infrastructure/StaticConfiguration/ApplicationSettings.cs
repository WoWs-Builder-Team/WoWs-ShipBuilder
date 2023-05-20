using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace WoWsShipBuilder.Desktop.Infrastructure.StaticConfiguration;

internal static class ApplicationSettings
{
    private static ApplicationOptions? instance;

    public static ApplicationOptions ApplicationOptions => instance ??= LoadOptions();

    private static ApplicationOptions LoadOptions()
    {
        var resourceName = "WoWsShipBuilder.Desktop.Infrastructure.StaticConfiguration.ApplicationOptions.json";
        using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        ApplicationOptions? result = null;
        if (stream != null)
        {
            using StreamReader reader = new(stream);
            string fileContent = reader.ReadToEnd();
            result = JsonConvert.DeserializeObject<ApplicationOptions>(fileContent);
        }

        return result ?? new ApplicationOptions();
    }
}
