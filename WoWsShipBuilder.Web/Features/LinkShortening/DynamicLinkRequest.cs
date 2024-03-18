using System.Text.Json.Serialization;

namespace WoWsShipBuilder.Web.Features.LinkShortening;

#pragma warning disable CA1720
internal enum LinkSuffixType
{
    SHORT,
    UNGUESSABLE,
}
#pragma warning restore CA1720

internal sealed record DynamicLinkRequest([property:JsonPropertyName("dynamicLinkInfo")] DynamicLinkInfo DynamicLinkInfo, [property:JsonPropertyName("suffix")] DynamicLinkSuffix Suffix);

internal sealed record DynamicLinkInfo([property:JsonPropertyName("domainUriPrefix")] string UriPrefix, [property:JsonPropertyName("link")] string Link);

internal sealed record DynamicLinkSuffix([property:JsonPropertyName("option")] LinkSuffixType Option);

internal sealed record DynamicLinkResponse(string ShortLink, string PreviewLink);
