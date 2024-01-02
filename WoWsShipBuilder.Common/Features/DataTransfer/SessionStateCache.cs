using System.Collections.Immutable;

namespace WoWsShipBuilder.Features.DataTransfer;

/**
 * <summary>
 * A caching service that stores <see cref="ShipBuildContainer"/> instances when navigating between pages.
 * </summary>
 */
public class SessionStateCache
{
    private ImmutableList<ShipBuildContainer>? buildTransferContainers;

    public ImmutableList<ShipBuildContainer>? GetAndResetBuildTransferContainers()
    {
        var result = this.buildTransferContainers;
        this.buildTransferContainers = null;
        return result;
    }

    public void SetBuildTransferContainers(ImmutableList<ShipBuildContainer> containers)
    {
        this.buildTransferContainers = containers;
    }

    public void SetBuildTransferContainers(ShipBuildContainer container)
    {
        this.buildTransferContainers = ImmutableList.Create(container);
    }
}
