using System.IO.Abstractions;
using WoWsShipBuilder.Infrastructure.HttpClients;

namespace WoWsShipBuilder.Desktop.CrossPlatform.Infrastructure.AwsClient;

public interface IDesktopAwsClient : IAwsClient
{
    Task DownloadImages(IFileSystem fileSystem, string? fileName = null);
}
