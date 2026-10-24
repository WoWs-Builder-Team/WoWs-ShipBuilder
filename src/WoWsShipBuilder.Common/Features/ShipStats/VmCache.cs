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

    /// <summary>
    /// Removes the entry for the specified id and disposes its viewmodel.
    /// </summary>
    /// <param name="id">The id of the entry to remove.</param>
    /// <returns><see langword="true"/> if an entry was removed, <see langword="false"/> otherwise.</returns>
    public bool RemoveEntry(Guid id)
    {
        if (!this.cacheEntries.Remove(id, out var removedEntry))
        {
            return false;
        }

        removedEntry?.ViewModel.Dispose();
        return true;
    }

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
