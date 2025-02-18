namespace WoWsShipBuilder.Features.ShipComparison.GridData;

public class PlaneGridDataWrapper
{
    public NoSortList<string> Type { get; protected init; } = new();

    public NoSortList<int> InSquadron { get; protected init; } = new();

    public NoSortList<int> PerAttack { get; protected init; } = new();

    public NoSortList<int> OnDeck { get; protected init; } = new();

    public NoSortList<decimal> RestorationTime { get; protected init; } = new();

    public NoSortList<decimal> CruisingSpeed { get; protected init; } = new();

    public NoSortList<decimal> MaxSpeed { get; protected init; } = new();

    public NoSortList<decimal> MinSpeed { get; protected init; } = new();

    public NoSortList<decimal> EngineBoostDuration { get; protected init; } = new();

    public NoSortList<decimal> InitialBoostDuration { get; protected init; } = new();

    public NoSortList<decimal> InitialBoostValue { get; protected init; } = new();

    public NoSortList<int> PlaneHp { get; protected init; } = new();

    public NoSortList<int> SquadronHp { get; protected init; } = new();

    public NoSortList<int> AttackGroupHp { get; protected init; } = new();

    public NoSortList<int> DamageDuringAttack { get; protected init; } = new();

    public NoSortList<int> WeaponsPerPlane { get; protected init; } = new();

    public NoSortList<decimal> PreparationTime { get; protected init; } = new();

    public NoSortList<decimal> AimingTime { get; protected init; } = new();

    public NoSortList<decimal> TimeToFullyAimed { get; protected init; } = new();

    public NoSortList<decimal> PostAttackInvulnerability { get; protected init; } = new();

    public NoSortList<decimal> AttackCooldown { get; protected init; } = new();

    public NoSortList<decimal> Concealment { get; protected init; } = new();

    public NoSortList<decimal> Spotting { get; protected init; } = new();

    public NoSortList<string> AreaChangeAiming { get; protected init; } = new();

    public NoSortList<string> AreaChangePreparation { get; protected init; } = new();
}
