using System.Text.Json.Serialization;

namespace WoWsShipBuilder.Features.LinkShortening;

#pragma warning disable CA1720
[Obsolete("Use Shlink link shortening instead")]
public enum LinkSuffixType
{
    SHORT,
    UNGUESSABLE,
}
#pragma warning restore CA1720

[Obsolete("Use Shlink link shortening instead")]
public record DynamicLinkRequest([property:JsonPropertyName("dynamicLinkInfo")] DynamicLinkInfo DynamicLinkInfo, [property:JsonPropertyName("suffix")] DynamicLinkSuffix Suffix);

[Obsolete("Use Shlink link shortening instead")]
public record DynamicLinkInfo([property:JsonPropertyName("domainUriPrefix")] string UriPrefix, [property:JsonPropertyName("link")] string Link);

[Obsolete("Use Shlink link shortening instead")]
public record DynamicLinkSuffix([property:JsonPropertyName("option")] LinkSuffixType Option);

[Obsolete("Use Shlink link shortening instead")]
public record DynamicLinkResponse(string ShortLink, string PreviewLink);
