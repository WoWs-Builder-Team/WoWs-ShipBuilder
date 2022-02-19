using System.Threading.Tasks;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.Services
{
    public interface IScreenshotRenderService
    {
        // TODO: get rid of output path
        public Task CreateBuildImageAsync(Build build, Ship rawShipData, bool includeSignals, string outputPath, bool copyToClipboard);
    }
}
