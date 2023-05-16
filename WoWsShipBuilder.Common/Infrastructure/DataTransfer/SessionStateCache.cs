namespace WoWsShipBuilder.Common.Infrastructure.DataTransfer;

/**
 * <summary>
 * A caching service that stores <see cref="ShipBuildContainer"/> instances when navigating between pages.
 * </summary>
 */
public class SessionStateCache
{
    private List<ShipBuildContainer>? buildTransferContainers;

    public List<ShipBuildContainer>? GetAndResetBuildTransferContainers()
    {
        var result = buildTransferContainers;
        buildTransferContainers = null;
        return result;
    }

    public void SetBuildTransferContainers(List<ShipBuildContainer> containers)
    {
        buildTransferContainers = containers;
    }

    public void SetBuildTransferContainers(ShipBuildContainer container)
    {
        buildTransferContainers = new() { container };
    }
}
