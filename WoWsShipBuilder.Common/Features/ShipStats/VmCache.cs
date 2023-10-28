using WoWsShipBuilder.Features.ShipStats.ViewModels;

namespace WoWsShipBuilder.Features.ShipStats;

public class VmCache
{
    private readonly Dictionary<Guid, VmCacheEntry?> cacheEntries = new();

    public VmCacheEntry? this[Guid id]
    {
        get => this.cacheEntries.GetValueOrDefault(id, default);
        set => this.cacheEntries[id] = value;
    }

    public VmCacheEntry? GetOrDefault(Guid id) => this.cacheEntries.GetValueOrDefault(id, default);

    public bool RemoveEntry(Guid id) => this.cacheEntries.Remove(id);
}

public sealed record VmCacheEntry(ShipViewModel ViewModel, string BuildName = "");
