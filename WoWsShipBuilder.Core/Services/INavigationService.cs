using System.Threading.Tasks;
using WoWsShipBuilder.Core.Builds;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Core.Services
{
    public interface INavigationService
    {
        public void OpenStartMenu(bool closeMainWindow = false);

        public Task OpenMainWindow(Ship ship, ShipSummary summary, Build? build = null, bool closeMainWindow = false);

        public void OpenDispersionPlotWindow(bool closeCurrentWindow = false);
    }
}
