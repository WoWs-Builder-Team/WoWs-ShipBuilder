namespace WoWsShipBuilder.Infrastructure.DataTransfer;

/// <summary>
/// A caching service that stores ShipBuildContainer instances when navigating between pages.
/// </summary>
public class SessionStateCache
{
    private List<ShipBuildContainer>? buildTransferContainers;

    public List<ShipBuildContainer>? GetAndResetBuildTransferContainers()
    {
        var result = this.buildTransferContainers;
        this.buildTransferContainers = null;
        return result;
    }

    public void SetBuildTransferContainers(List<ShipBuildContainer> containers)
    {
        this.buildTransferContainers = containers;
    }

    public void SetBuildTransferContainers(ShipBuildContainer container)
    {
        this.buildTransferContainers = new() { container };
    }
}
