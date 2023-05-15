using System.Globalization;

namespace WoWsShipBuilder.Common.Infrastructure;

public static class AppConstants
{
#if DEBUG
    public const string ShipBuilderName = "WoWsShipBuilderDev";
#else
    public const string ShipBuilderName = "WoWsShipBuilder";
#endif

    // Workaround for webworkers
    static AppConstants()
    {
        try
        {
            DefaultCultureDetails = new(new("en-GB"), "en");
            SupportedLanguages = new List<CultureDetails>
            {
                DefaultCultureDetails,
                new(new("nl-NL"), "nl"),
                new(new("fr-FR"), "fr"),
                new(new("de-DE"), "de"),
                new(new("it-IT"), "it"),
                new(new("ja-JP"), "ja"),
                new(new("pt-BR"), "pt_br"),
                new(new("ru-RU"), "ru"),
                new(new("es-ES"), "es"),
                new(new("tr-TR"), "tr"),
                new(new("hu-HU"), "en"),
            };
        }
        catch (Exception)
        {
            DefaultCultureDetails = new(CultureInfo.InvariantCulture, "en");
            SupportedLanguages = new List<CultureDetails>
            {
                DefaultCultureDetails,
            };
        }
    }

    public static CultureDetails DefaultCultureDetails { get; }

    public static IEnumerable<CultureDetails> SupportedLanguages { get; }
}
