using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace WoWsShipBuilder.Features.LinkShortening;

public sealed record ShlinkRequest(
    [property:JsonPropertyName("longUrl")]
    string LongUrl,
    [property:JsonPropertyName("title")]
    string Title,
    [property:JsonPropertyName("domain")]
    string Domain,
    [property:JsonPropertyName("findIfExists")]
    bool FindIfExists,
    [property:JsonPropertyName("tags")]
    ImmutableArray<string> Tags = default,
    [property:JsonPropertyName("customSlug")]
    string? CustomSlug = null,
    [property:JsonPropertyName("forwardQuery")]
    bool ForwardQuery = false
);

public sealed record ShlinkResponse(
    [property:JsonPropertyName("shortCode")]
    string ShortCode,
    [property:JsonPropertyName("shortUrl")]
    string ShortUrl
);
