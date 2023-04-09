using WoWsShipBuilder.Web.Data;

namespace WoWsShipBuilder.Web.Services;

/**
 * A caching service that stores <see cref="BuildTransferContainer"/> instances when navigating between pages.
 */
public class SessionStateCache
{
    private List<BuildTransferContainer>? buildTransferContainers;

    public List<BuildTransferContainer>? GetAndResetBuildTransferContainers()
    {
        var result = buildTransferContainers;
        buildTransferContainers = null;
        return result;
    }

    public void SetBuildTransferContainers(List<BuildTransferContainer> containers)
    {
        buildTransferContainers = containers;
    }

    public void SetBuildTransferContainers(BuildTransferContainer container)
    {
        buildTransferContainers = new() { container };
    }
}
