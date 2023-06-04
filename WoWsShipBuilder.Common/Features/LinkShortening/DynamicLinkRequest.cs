using System.Text.Json.Serialization;

namespace WoWsShipBuilder.Features.LinkShortening;

public enum LinkSuffixType
{
    SHORT,
    UNGUESSABLE,
}

public record DynamicLinkRequest([property:JsonPropertyName("dynamicLinkInfo")] DynamicLinkInfo DynamicLinkInfo, [property:JsonPropertyName("suffix")] DynamicLinkSuffix Suffix);

public record DynamicLinkInfo([property:JsonPropertyName("domainUriPrefix")] string UriPrefix, [property:JsonPropertyName("link")] string Link);

public record DynamicLinkSuffix([property:JsonPropertyName("option")] LinkSuffixType Option);

public record DynamicLinkResponse(string ShortLink, string PreviewLink);
