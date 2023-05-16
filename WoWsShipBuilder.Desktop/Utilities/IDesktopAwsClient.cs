using System.IO.Abstractions;
using System.Threading.Tasks;
using WoWsShipBuilder.Infrastructure.HttpClients;

namespace WoWsShipBuilder.Desktop.Utilities;

public interface IDesktopAwsClient : IAwsClient
{
    Task DownloadImages(IFileSystem fileSystem, string? fileName = null);
}
