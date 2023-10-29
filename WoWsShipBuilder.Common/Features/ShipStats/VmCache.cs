using WoWsShipBuilder.Features.ShipStats.ViewModels;

namespace WoWsShipBuilder.Features.ShipStats;

public sealed class VmCache : IDisposable
{
    private readonly Dictionary<Guid, VmCacheEntry?> cacheEntries = new();

    public VmCacheEntry? this[Guid id]
    {
        get => this.cacheEntries.GetValueOrDefault(id, default);
        set => this.cacheEntries[id] = value;
    }

    public VmCacheEntry? GetOrDefault(Guid id) => this.cacheEntries.GetValueOrDefault(id, default);

    public bool RemoveEntry(Guid id) => this.cacheEntries.Remove(id);

    public void Dispose()
    {
        foreach (var cacheEntry in this.cacheEntries)
        {
            cacheEntry.Value?.ViewModel.Dispose();
        }

        this.cacheEntries.Clear();
    }
}

public sealed record VmCacheEntry(ShipViewModel ViewModel, string BuildName = "");
