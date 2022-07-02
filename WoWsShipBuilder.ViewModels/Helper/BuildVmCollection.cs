using WoWsShipBuilder.ViewModels.ShipVm;

namespace WoWsShipBuilder.ViewModels.Helper;

public sealed record BuildVmCollection(
    string BuildName, string ShipIndex, ShipModuleViewModel ModulesVm, ConsumableViewModel ConsumableVm, CaptainSkillSelectorViewModel CaptainVm, UpgradePanelViewModelBase UpgradesVm, SignalSelectorViewModel SignalVm);
