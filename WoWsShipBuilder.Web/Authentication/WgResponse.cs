using System.Text.Json.Serialization;

namespace WoWsShipBuilder.Web.Authentication;

internal class WgResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public Dictionary<string, Data?> Data { get; set; } = new();
}

internal class Data
{
    [JsonPropertyName("private")]
    public Dictionary<string, object>? Private { get; set; }
}
