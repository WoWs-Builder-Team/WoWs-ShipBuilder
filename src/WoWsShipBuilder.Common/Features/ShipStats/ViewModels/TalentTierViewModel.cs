using System.Collections.Immutable;
using WoWsShipBuilder.DataStructures.Modifiers;

namespace WoWsShipBuilder.Features.ShipStats.ViewModels;

/// <summary>
/// One escalation step of a tiered captain talent.
/// </summary>
/// <param name="Level">Tier number as reported by the game data.</param>
/// <param name="Modifiers">
/// The effective stats while the talent sits at this tier. These are absolute at the tier and must not be compounded
/// across activations the way a repeatable non-tiered talent's are.
/// </param>
/// <param name="ActivationThreshold">
/// What has to be reached for this tier, such as a number of ships destroyed or an amount of damage dealt. Null when
/// the game data states no threshold.
/// </param>
/// <param name="ActivationThresholdKey">
/// The game's name for <paramref name="ActivationThreshold"/>, which determines how it should be formatted: a count
/// and a share of maximum health are both plain numbers in the data but read very differently.
/// </param>
public sealed record TalentTierViewModel(int Level, ImmutableList<Modifier> Modifiers, decimal? ActivationThreshold, string ActivationThresholdKey = "");
