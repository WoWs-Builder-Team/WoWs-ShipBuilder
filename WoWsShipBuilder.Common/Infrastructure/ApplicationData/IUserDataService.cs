using WoWsShipBuilder.Features.Builds;

namespace WoWsShipBuilder.Infrastructure.ApplicationData;

public interface IUserDataService
{
    Task SaveBuildsAsync(IEnumerable<Build> builds);

    Task<IEnumerable<Build>> LoadBuildsAsync();

    Task SaveBuildAsync(Build build);

    Task ImportBuildsAsync(IEnumerable<Build> builds);

    Task RemoveSavedBuildAsync(Build build);

    Task RemoveSavedBuildsAsync(IEnumerable<Build> builds);
}
