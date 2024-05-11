using System.Collections.Concurrent;
using WoWsShipBuilder.Features.Settings;

namespace WoWsShipBuilder.Features.ShipStats;

public sealed class ExpanderStateCache
{
    private readonly AppSettings appSettings;

    private readonly ConcurrentDictionary<string, bool> expanderStates = new();

    public ExpanderStateCache(AppSettings appSettings)
    {
        this.appSettings = appSettings;
    }

    public bool this[string key]
    {
        get => this.expanderStates.GetOrAdd(key, this.ComputeInitialState);
        set => this.expanderStates[key] = value;
    }

    public void Reset() => this.expanderStates.Clear();

    private bool ComputeInitialState(string key)
    {
        if (key.StartsWith("main", StringComparison.Ordinal))
        {
            return this.appSettings.OpenAllMainExpandersByDefault;
        }

        if (key.StartsWith("ammo", StringComparison.Ordinal))
        {
            return this.appSettings.OpenAllAmmoExpandersByDefault;
        }

        if (key.StartsWith("sec", StringComparison.Ordinal))
        {
            return this.appSettings.OpenSecondariesAndAaExpandersByDefault;
        }

        return false;
    }
}
