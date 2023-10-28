using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.Features.DataContainers;

namespace WoWsShipBuilder.Features.ShipComparison.GridData;

public class TorpedoGridDataWrapper
{
    public TorpedoGridDataWrapper(TorpedoArmamentDataContainer? torpedoArmament)
    {
        List<TorpedoDataContainer>? torpedoes = torpedoArmament?.Torpedoes;

        this.FullSalvoDamage = new() { torpedoArmament?.FullSalvoDamage, torpedoArmament?.TorpFullSalvoDmg, torpedoArmament?.AltTorpFullSalvoDmg };
        this.Type = torpedoes?.Select(x => x.TorpedoType).ToList() ?? new();
        this.Damage = torpedoes?.Select(x => x.Damage).ToList() ?? new();
        this.Range = torpedoes?.Select(x => x.Range).ToList() ?? new();
        this.Speed = torpedoes?.Select(x => x.Speed).ToList() ?? new();
        this.DetectRange = torpedoes?.Select(x => x.Detectability).ToList() ?? new();
        this.ArmingDistance = torpedoes?.Select(x => x.ArmingDistance).ToList() ?? new();
        this.ReactionTime = torpedoes?.Select(x => x.ReactionTime).ToList() ?? new();
        this.FloodingChance = torpedoes?.Select(x => x.FloodingChance).ToList() ?? new();
        this.BlastRadius = torpedoes?.Select(x => x.ExplosionRadius).ToList() ?? new();
        this.BlastPenetration = torpedoes?.Select(x => x.SplashCoeff).ToList() ?? new();
        this.CanHit = torpedoes?.Select(x => x.CanHitClasses).ToList() ?? new();
    }

    public List<string?> FullSalvoDamage { get; }

    public List<string> Type { get; }

    public List<decimal> Damage { get; }

    public List<decimal> Range { get; }

    public List<decimal> Speed { get; }

    public List<decimal> DetectRange { get; }

    public List<int> ArmingDistance { get; }

    public List<decimal> ReactionTime { get; }

    public List<decimal> FloodingChance { get; }

    public List<decimal> BlastRadius { get; }

    public List<decimal> BlastPenetration { get; }

    public List<List<ShipClass>?> CanHit { get; }
}
