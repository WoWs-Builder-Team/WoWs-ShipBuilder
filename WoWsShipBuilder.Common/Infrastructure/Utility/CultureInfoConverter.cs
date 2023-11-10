using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WoWsShipBuilder.Infrastructure.Utility;

public sealed class CultureInfoConverter : JsonConverter<CultureInfo>
{
    public override CultureInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return CultureInfo.CreateSpecificCulture(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, CultureInfo value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Name);
    }
}
