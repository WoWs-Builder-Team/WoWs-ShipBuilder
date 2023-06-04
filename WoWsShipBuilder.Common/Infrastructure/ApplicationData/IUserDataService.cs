using WoWsShipBuilder.Features.Builds;

namespace WoWsShipBuilder.Infrastructure.ApplicationData;

public interface IUserDataService
{
    Task SaveBuildsAsync(IEnumerable<Build> builds);

    Task<IEnumerable<Build>> LoadBuildsAsync();

    Task AddRecentBuildAsync(Build build);

    Task RemoveRecentBuildAsync(Build build);
}
