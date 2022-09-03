using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Web.Data;

public sealed record ShipBuildContainer(Ship Ship, Build? Build, Guid Id, IEnumerable<int>? ActivatedConsumableSlots, bool SpecialAbilityActive, ShipDataContainer? ShipDataContainer)
{
    public static ShipBuildContainer CreateNew(Ship ship, Build? build, IEnumerable<int>? activatedConsumableSlots, bool specialAbilityActive = false, ShipDataContainer? shipDataContainer = null)
    {
        return new(ship, build, Guid.NewGuid(), activatedConsumableSlots, specialAbilityActive, shipDataContainer);
    }
}
