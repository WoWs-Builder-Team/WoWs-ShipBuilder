using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using WoWsShipBuilder.Features.DataTransfer;

namespace WoWsShipBuilder.Infrastructure.ApplicationData;

public static class AppConstants
{
#if DEBUG
    public const string ShipBuilderName = "WoWsShipBuilderDev";
#else
    public const string ShipBuilderName = "WoWsShipBuilder";
#endif

    public const string AdminRoleName = "admin";

    public const string BuildCuratorRoleName = "build-curator";

    public static CultureDetails DefaultCultureDetails { get; } = new(new("en-GB"), "en");

    public static ImmutableArray<CultureDetails> SupportedLanguages { get; } = ImmutableArray.Create(
        DefaultCultureDetails,
        new(new("zh-CN"), "zh"),
        new(new("zh-TW"), "zh_tw"),
        new(new("nl-NL"), "nl"),
        new(new("fr-FR"), "fr"),
        new(new("de-DE"), "de"),
        new(new("hu-HU"), "en"),
        new(new("it-IT"), "it"),
        new(new("ja-JP"), "ja"),
        new(new("pt-BR"), "pt_br"),
        new(new("ru-RU"), "ru"),
        new(new("es-ES"), "es"),
        new(new("tr-TR"), "tr"));

    public static JsonSerializerOptions JsonSerializerOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}
