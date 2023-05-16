﻿using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Features.Builds;

namespace WoWsShipBuilder.Infrastructure
{
    public interface INavigationService
    {
        public void OpenStartMenu(bool closeMainWindow = false);

        public Task OpenMainWindow(Ship ship, ShipSummary summary, Build? build = null, bool closeMainWindow = false);

        public void OpenDispersionPlotWindow(bool closeCurrentWindow = false);
    }
}
