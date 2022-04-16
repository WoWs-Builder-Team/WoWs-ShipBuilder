using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.HttpResponses;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.HttpClients
{
    public interface IAwsClient
    {
        public Task<VersionInfo> DownloadVersionInfo(ServerType serverType);

        public Task DownloadFiles(ServerType serverType, List<(string, string)> relativeFilePaths, IProgress<int>? downloadProgress = null);

        [UnsupportedOSPlatform("browser")]
        public Task DownloadImages(ImageType type, IFileSystem fileSystem, string? fileName = null);
    }
}
