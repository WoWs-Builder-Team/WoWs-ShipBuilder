using System.Collections.Immutable;
using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure.ApplicationData;

namespace WoWsShipBuilder.Features.DataContainers;

[DataContainer]
public partial class AirstrikeDataContainer : DataContainerBase
{
    public string Header { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValue, ValueTextKind = TextKind.LocalizationKey)]
    public string Name { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "HP")]
    public int PlaneHp { get; set; }

    [DataElementType(DataElementTypes.KeyValue)]
    public int NumberOfUses { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Range", UnitKey = "KM")]
    public decimal MaximumDistance { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Range", UnitKey = "KM")]
    public decimal MinimumDistance { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "KM")]
    public decimal MaximumFlightDistance { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Timings", UnitKey = "S")]
    public decimal FlyAwayTime { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Timings", UnitKey = "S")]
    public decimal DropTime { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Timings", UnitKey = "S")]
    public decimal ReloadTime { get; set; }

    [DataElementType(DataElementTypes.KeyValue)]
    public int NumberDuringAttack { get; set; }

    [DataElementType(DataElementTypes.KeyValue)]
    public int BombsPerPlane { get; set; }

    public string WeaponType { get; set; } = null!;

    public ProjectileDataContainer? Weapon { get; set; }

    public static AirstrikeDataContainer? FromShip(Ship ship, List<Modifier> modifiers, bool isAsw)
    {
        string header = isAsw ? "ShipStats_AswAirstrike" : "ShipStats_Airstrike";
        ImmutableDictionary<string, AirStrike> airstrikes = ship.AirStrikes;
        if (ship.AirStrikes.Count == 0)
        {
            return null;
        }

        var airstrike = airstrikes.FirstOrDefault().Value;
        var plane = AppData.FindAircraft(airstrike.PlaneName[..airstrike.PlaneName.IndexOf("_", StringComparison.InvariantCultureIgnoreCase)]);

        if (isAsw != plane.PlaneCategory.Equals(PlaneCategory.Asw))
        {
            return null;
        }

        decimal reload = modifiers.ApplyModifiers("AirstrikeDataContainer.Reload", airstrike.ReloadTime);
        var uses = modifiers.ApplyModifiers("AirstrikeDataContainer.Uses", airstrike.Charges);

        decimal planeHp = (decimal)plane.MaxHealth;
        var finalPlaneHp = modifiers.ApplyModifiers("AirstrikeDataContainer.PlaneHp", planeHp);

        ProjectileDataContainer? weapon;
        string weaponType;
        if (isAsw)
        {
            weapon = DepthChargeDataContainer.FromChargesName(plane.BombName, modifiers);
            weaponType = ProjectileType.DepthCharge.ToString();
        }
        else
        {
            weapon = BombDataContainer.FromBombName(plane.BombName, modifiers);
            weaponType = ProjectileType.Bomb.ToString();
        }

        var airstrikeDataContainer = new AirstrikeDataContainer
        {
            Header = header,
            Name = airstrike.PlaneName,
            PlaneHp = (int)Math.Round(finalPlaneHp, 0),
            ReloadTime = Math.Round(reload, 2),
            NumberOfUses = uses,
            DropTime = airstrike.DropTime,
            FlyAwayTime = airstrike.FlyAwayTime,
            MaximumDistance = Math.Round((decimal)airstrike.MaximumDistance / 1000, 2),
            MaximumFlightDistance = Math.Round((decimal)airstrike.MaximumFlightDistance / 1000, 2),
            MinimumDistance = Math.Round((decimal)airstrike.MinimumDistance / 1000, 2),
            BombsPerPlane = plane.AttackData.AttackCount,
            NumberDuringAttack = plane.AttackData.AttackerSize,
            WeaponType = weaponType,
            Weapon = weapon,
        };

        airstrikeDataContainer.UpdateDataElements();

        return airstrikeDataContainer;
    }
}
