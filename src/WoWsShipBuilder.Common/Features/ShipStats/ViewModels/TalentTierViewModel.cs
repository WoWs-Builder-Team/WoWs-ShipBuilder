using System.Collections.Immutable;
using WoWsShipBuilder.DataStructures.Modifiers;

namespace WoWsShipBuilder.Features.ShipStats.ViewModels;

/// <summary>
/// One escalation step of a tiered captain talent.
/// </summary>
/// <param name="Level">1-based tier number.</param>
/// <param name="Modifiers">
/// The effective stats while the talent sits at this tier. These are absolute at the tier and must not be compounded
/// across activations the way a repeatable non-tiered talent's are.
/// </param>
public sealed record TalentTierViewModel(int Level, ImmutableList<Modifier> Modifiers);
