using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using WoWsShipBuilder.Core.DataProvider;

namespace WoWsShipBuilder.Core.Data;

public class GameLocalizationResourceManager
{
    private readonly string localizationDirectory;

    private readonly IFileSystem fileSystem;

    private readonly ConcurrentDictionary<string, Dictionary<string, string>> localizations;

    private CultureDetails currentCulture;

    public GameLocalizationResourceManager(IFileSystem fileSystem, string localizationDirectory, CultureDetails initialCulture)
    {
        this.fileSystem = fileSystem;
        this.localizationDirectory = localizationDirectory;
        currentCulture = initialCulture;
        localizations = new();

        LoadAndRegisterLocalization(initialCulture);
    }

    public CultureDetails CurrentCulture
    {
        get => currentCulture;
        set
        {
            currentCulture = value;
            LoadAndRegisterLocalization(value);
        }
    }

    public IEnumerable<string> LoadedLocalizations => localizations.Keys.ToList();

    public string GetString(string key, CultureDetails? cultureDetails = null)
    {
        cultureDetails ??= currentCulture;

        if (!localizations.TryGetValue(cultureDetails.LocalizationFileName, out var loc))
        {
            loc = LoadAndRegisterLocalization(cultureDetails);
        }

        return loc.TryGetValue(key, out var result) ? result : key;
    }

    private Dictionary<string, string> LoadAndRegisterLocalization(CultureDetails cultureDetails)
    {
        string? jsonContent = fileSystem.File.ReadAllText(fileSystem.Path.Combine(localizationDirectory, cultureDetails.LocalizationFileName + ".json"));
        var loc = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent) ?? new();
        localizations[cultureDetails.LocalizationFileName] = loc;
        return loc;
    }
}
