using System.Runtime.Versioning;
using WoWsShipBuilder.Common.Infrastructure.GameData;
using WoWsShipBuilder.DataStructures.Versioning;

namespace WoWsShipBuilder.Common.Infrastructure.HttpClients;

public interface IAwsClient
{
    public Task<VersionInfo> DownloadVersionInfo(ServerType serverType);

    public Task DownloadFiles(ServerType serverType, List<(string, string)> relativeFilePaths, IProgress<int>? downloadProgress = null);
}
