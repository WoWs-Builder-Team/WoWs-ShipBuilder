using System.Collections.Immutable;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Features.Builds;
using WoWsShipBuilder.Features.DataContainers;

namespace WoWsShipBuilder.Features.Navigation;

public sealed record ShipBuildContainer(Ship Ship, Build? Build, Guid Id, ImmutableArray<int> ActivatedConsumableSlots, bool SpecialAbilityActive, ShipDataContainer? ShipDataContainer, ImmutableList<Modifier> Modifiers)
{
    public static ShipBuildContainer CreateNew(Ship ship, Build? build, ImmutableArray<int> activatedConsumableSlots, bool specialAbilityActive = false)
    {
        return new(ship, build, Guid.NewGuid(), activatedConsumableSlots, specialAbilityActive, null, ImmutableList<Modifier>.Empty);
    }

    /// <summary>
    /// Compares this ShipBuilderContainer with another object and determines whether their value is equivalent.
    /// <br/>
    /// Equivalence is determined based on the <see cref="Ship"/>, <see cref="Build"/>, <see cref="ActivatedConsumableSlots"/> and <see cref="SpecialAbilityActive"/> properties.
    /// <br/>
    /// The <see cref="ShipDataContainer"/> and <see cref="Modifiers"/> properties are ignored because they are generated based on the other values and therefore not needed for an equivalence check.
    /// <br/>
    /// The <see cref="Guid"/> property is also irrelevant because two different records can have the same configuration.
    /// </summary>
    /// <param name="newContainer">The other object to compare with this record.</param>
    /// <returns><see langword="true"/> if all relevant properties have the same value, <see langword="false"/> otherwise.</returns>
    public bool IsEquivalentTo(ShipBuildContainer newContainer)
    {
        if (this.Ship.Index != newContainer.Ship.Index)
        {
            return false;
        }

        if (this.Build is null && newContainer.Build is not null)
        {
            return false;
        }

        if (this.Build is not null && !this.Build.Equals(newContainer.Build))
        {
            return false;
        }

        if (this.SpecialAbilityActive != newContainer.SpecialAbilityActive)
        {
            return false;
        }

        if ((this.ActivatedConsumableSlots.IsEmpty && !newContainer.ActivatedConsumableSlots.IsEmpty) || (!this.ActivatedConsumableSlots.IsEmpty && newContainer.ActivatedConsumableSlots.IsEmpty))
        {
            return false;
        }

        IOrderedEnumerable<int> oldConsumables = this.ActivatedConsumableSlots.OrderBy(i => i);
        IOrderedEnumerable<int> newConsumables = newContainer.ActivatedConsumableSlots.OrderBy(i => i);
        return oldConsumables.SequenceEqual(newConsumables);
    }
}
