using WoWsShipBuilder.Web.ViewModels;

namespace WoWsShipBuilder.Web.Services;

public class VmCacheService
{
    private readonly Dictionary<Guid, VmCacheEntry?> cacheEntries = new();

    public VmCacheEntry? this[Guid id]
    {
        get => cacheEntries.GetValueOrDefault(id, default);
        set => cacheEntries[id] = value;
    }

    public VmCacheEntry? GetOrDefault(Guid id) => cacheEntries.GetValueOrDefault(id, default);

    public bool RemoveEntry(Guid id) => cacheEntries.Remove(id);
}

public sealed record VmCacheEntry(ShipViewModel ViewModel)
{
    public string BuildName { get; set; } = string.Empty;
}
