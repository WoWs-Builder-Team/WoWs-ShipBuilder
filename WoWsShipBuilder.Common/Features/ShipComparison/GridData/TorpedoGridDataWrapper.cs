using System.Collections.Immutable;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.Features.DataContainers;

namespace WoWsShipBuilder.Features.ShipComparison.GridData;

public class TorpedoGridDataWrapper
{
    public TorpedoGridDataWrapper(TorpedoArmamentDataContainer? torpedoArmament)
    {
        var torpedoes = torpedoArmament?.Torpedoes;

        this.FullSalvoDamage = new() { torpedoArmament?.FullSalvoDamage, torpedoArmament?.TorpFullSalvoDmg, torpedoArmament?.AltTorpFullSalvoDmg };
        this.Type = torpedoes?.Select(x => x.TorpedoType).ToNoSortList() ?? new();
        this.Damage = torpedoes?.Select(x => x.Damage).ToNoSortList() ?? new();
        this.Range = torpedoes?.Select(x => x.Range).ToNoSortList() ?? new();
        this.Speed = torpedoes?.Select(x => x.Speed).ToNoSortList() ?? new();
        this.DetectRange = torpedoes?.Select(x => x.Detectability).ToNoSortList() ?? new();
        this.ArmingDistance = torpedoes?.Select(x => x.ArmingDistance).ToNoSortList() ?? new();
        this.ReactionTime = torpedoes?.Select(x => x.ReactionTime).ToNoSortList() ?? new();
        this.FloodingChance = torpedoes?.Select(x => x.FloodingChance).ToNoSortList() ?? new();
        this.BlastRadius = torpedoes?.Select(x => x.ExplosionRadius).ToNoSortList() ?? new();
        this.BlastPenetration = torpedoes?.Select(x => x.SplashCoeff).ToNoSortList() ?? new();
        this.CanHit = torpedoes?.Select(x => x.CanHitClasses).ToNoSortList() ?? new();
    }

    public NoSortList<string?> FullSalvoDamage { get; }

    public NoSortList<string> Type { get; }

    public NoSortList<decimal> Damage { get; }

    public NoSortList<decimal> Range { get; }

    public NoSortList<decimal> Speed { get; }

    public NoSortList<decimal> DetectRange { get; }

    public NoSortList<int> ArmingDistance { get; }

    public NoSortList<decimal> ReactionTime { get; }

    public NoSortList<decimal> FloodingChance { get; }

    public NoSortList<decimal> BlastRadius { get; }

    public NoSortList<decimal> BlastPenetration { get; }

    public NoSortList<ImmutableList<ShipClass>> CanHit { get; }
}
