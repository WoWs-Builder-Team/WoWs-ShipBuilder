using System.Globalization;
using System.Text.Json.Serialization;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Infrastructure.DataTransfer;

public sealed record CultureDetails(
    [property: JsonConverter(typeof(CultureInfoConverter))]
    CultureInfo CultureInfo,
    string LocalizationFileName);
