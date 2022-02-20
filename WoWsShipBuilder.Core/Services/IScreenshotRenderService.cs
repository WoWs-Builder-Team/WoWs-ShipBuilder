using System.Threading.Tasks;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.Services
{
    public interface IScreenshotRenderService
    {
        public Task CreateBuildImageAsync(Build build, Ship rawShipData, bool includeSignals, bool copyToClipboard);
    }
}
