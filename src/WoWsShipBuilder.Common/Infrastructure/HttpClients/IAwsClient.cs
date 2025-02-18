using WoWsShipBuilder.DataStructures.Versioning;
using WoWsShipBuilder.Infrastructure.GameData;

namespace WoWsShipBuilder.Infrastructure.HttpClients;

public interface IAwsClient
{
    public Task<VersionInfo> DownloadVersionInfo(ServerType serverType);

    public Task DownloadFiles(ServerType serverType, List<(string, string)> relativeFilePaths, IProgress<int>? downloadProgress = null);
}
