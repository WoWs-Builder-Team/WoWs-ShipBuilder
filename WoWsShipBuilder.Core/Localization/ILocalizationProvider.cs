﻿using System.Threading.Tasks;
using WoWsShipBuilder.Core.DataProvider;

namespace WoWsShipBuilder.Core.Localization;

public interface ILocalizationProvider
{
    Task RefreshDataAsync(params CultureDetails[] supportedCultures);

    string? GetString(string key, CultureDetails cultureDetails);
}
