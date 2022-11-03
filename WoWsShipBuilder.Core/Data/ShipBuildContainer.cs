using System;
using System.Collections.Generic;
using WoWsShipBuilder.Core.Builds;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Core.Data;

public sealed record ShipBuildContainer(Ship Ship, Build? Build, Guid Id, IEnumerable<int>? ActivatedConsumableSlots, bool SpecialAbilityActive, ShipDataContainer? ShipDataContainer, List<(string, float)>? Modifiers)
{
    public static ShipBuildContainer CreateNew(Ship ship, Build? build, IEnumerable<int>? activatedConsumableSlots, bool specialAbilityActive = false)
    {
        return new(ship, build, Guid.NewGuid(), activatedConsumableSlots, specialAbilityActive, null, null);
    }
}
