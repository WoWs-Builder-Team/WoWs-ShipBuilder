using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.HttpResponses;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.HttpClients
{
    public interface IAwsClient
    {
        public Task<VersionInfo> DownloadVersionInfo(ServerType serverType);

        public Task DownloadFiles(ServerType serverType, List<(string, string)> relativeFilePaths, IProgress<int>? downloadProgress = null);

        public Task DownloadImages(ImageType type, string? fileName = null);
    }
}
