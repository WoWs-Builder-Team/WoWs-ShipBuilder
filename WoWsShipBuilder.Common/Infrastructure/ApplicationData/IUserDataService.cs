namespace WoWsShipBuilder.Infrastructure.ApplicationData;

public interface IUserDataService
{
    /// <summary>
    /// Save string compressed <see cref="Build"/> to the disk.
    /// </summary>
    void SaveBuilds();

    void LoadBuilds();
}
