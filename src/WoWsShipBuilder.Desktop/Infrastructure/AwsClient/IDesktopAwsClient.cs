using System.IO.Abstractions;
using System.Threading.Tasks;
using WoWsShipBuilder.Infrastructure.HttpClients;

namespace WoWsShipBuilder.Desktop.Infrastructure.AwsClient;

public interface IDesktopAwsClient : IAwsClient
{
    Task DownloadImages(IFileSystem fileSystem, string? fileName = null);
}
