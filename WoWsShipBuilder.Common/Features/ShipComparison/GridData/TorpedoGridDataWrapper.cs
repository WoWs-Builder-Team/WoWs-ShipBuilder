using WoWsShipBuilder.DataContainers;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Features.ShipComparison.GridData;

public class TorpedoGridDataWrapper
{
    public TorpedoGridDataWrapper(TorpedoArmamentDataContainer? torpedoArmament)
    {
        List<TorpedoDataContainer>? torpedoes = torpedoArmament?.Torpedoes;

        FullSalvoDamage = new() { torpedoArmament?.FullSalvoDamage, torpedoArmament?.TorpFullSalvoDmg, torpedoArmament?.AltTorpFullSalvoDmg };
        Type = torpedoes?.Select(x => x.TorpedoType).ToList() ?? new();
        Damage = torpedoes?.Select(x => x.Damage).ToList() ?? new();
        Range = torpedoes?.Select(x => x.Range).ToList() ?? new();
        Speed = torpedoes?.Select(x => x.Speed).ToList() ?? new();
        DetectRange = torpedoes?.Select(x => x.Detectability).ToList() ?? new();
        ArmingDistance = torpedoes?.Select(x => x.ArmingDistance).ToList() ?? new();
        ReactionTime = torpedoes?.Select(x => x.ReactionTime).ToList() ?? new();
        FloodingChance = torpedoes?.Select(x => x.FloodingChance).ToList() ?? new();
        BlastRadius = torpedoes?.Select(x => x.ExplosionRadius).ToList() ?? new();
        BlastPenetration = torpedoes?.Select(x => x.SplashCoeff).ToList() ?? new();
        CanHit = torpedoes?.Select(x => x.CanHitClasses).ToList() ?? new();
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
