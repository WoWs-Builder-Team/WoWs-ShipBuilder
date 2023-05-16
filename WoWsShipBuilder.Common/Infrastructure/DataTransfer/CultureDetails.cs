using System.Globalization;

namespace WoWsShipBuilder.Infrastructure.DataTransfer;

public sealed record CultureDetails(CultureInfo CultureInfo, string LocalizationFileName);
