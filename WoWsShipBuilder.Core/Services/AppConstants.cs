using System.Collections.Generic;
using WoWsShipBuilder.Core.DataProvider;

namespace WoWsShipBuilder.Core.Services
{
    public static class AppConstants
    {
        public static CultureDetails DefaultCultureDetails { get; } = new(new("en-GB"), "en");

        public static IEnumerable<CultureDetails> SupportedLanguages { get; } = new List<CultureDetails>
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
        };
    }
}
