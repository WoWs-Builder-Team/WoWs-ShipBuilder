using System;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.ViewModels.Helper;

public sealed record ShipComparisonDataWrapper(Ship Ship, ShipDataContainer ShipDataContainer, Build? Build = null)
{
    public ShipComparisonDataWrapper(Ship ship, ShipDataContainer shipDataContainer, Build? build, Guid id)
        : this(ship, shipDataContainer, build)
    {
        Id = id;
    }

    public Guid Id { get; } = Guid.NewGuid();
}
