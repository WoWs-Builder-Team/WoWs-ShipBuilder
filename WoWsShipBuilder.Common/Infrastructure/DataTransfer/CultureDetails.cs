using System.Globalization;

namespace WoWsShipBuilder.Common.Infrastructure.DataTransfer;

public sealed record CultureDetails(CultureInfo CultureInfo, string LocalizationFileName);
