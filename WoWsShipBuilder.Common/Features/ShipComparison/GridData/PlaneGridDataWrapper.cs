namespace WoWsShipBuilder.Features.ShipComparison.GridData;

public class PlaneGridDataWrapper
{
    public List<string> Type { get; protected init; } = new();

    public List<int> InSquadron { get; protected init; } = new();

    public List<int> PerAttack { get; protected init; } = new();

    public List<int> OnDeck { get; protected init; } = new();

    public List<decimal> RestorationTime { get; protected init; } = new();

    public List<decimal> CruisingSpeed { get; protected init; } = new();

    public List<decimal> MaxSpeed { get; protected init; } = new();

    public List<decimal> MinSpeed { get; protected init; } = new();

    public List<decimal> EngineBoostDuration { get; protected init; } = new();

    public List<decimal> InitialBoostDuration { get; protected init; } = new();

    public List<decimal> InitialBoostValue { get; protected init; } = new();

    public List<int> PlaneHp { get; protected init; } = new();

    public List<int> SquadronHp { get; protected init; } = new();

    public List<int> AttackGroupHp { get; protected init; } = new();

    public List<int> DamageDuringAttack { get; protected init; } = new();

    public List<int> WeaponsPerPlane { get; protected init; } = new();

    public List<decimal> PreparationTime { get; protected init; } = new();

    public List<decimal> AimingTime { get; protected init; } = new();

    public List<decimal> TimeToFullyAimed { get; protected init; } = new();

    public List<decimal> PostAttackInvulnerability { get; protected init; } = new();

    public List<decimal> AttackCooldown { get; protected init; } = new();

    public List<decimal> Concealment { get; protected init; } = new();

    public List<decimal> Spotting { get; protected init; } = new();

    public List<string> AreaChangeAiming { get; protected init; } = new();

    public List<string> AreaChangePreparation { get; protected init; } = new();
}
