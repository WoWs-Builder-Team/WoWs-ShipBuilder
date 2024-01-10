using System.Globalization;
using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Aircraft;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.GameData;

namespace WoWsShipBuilder.Features.DataContainers;

[DataContainer]
public partial class CvAircraftDataContainer : DataContainerBase
{
    public string Name { get; set; } = default!;

    public string PlaneVariant { get; set; } = default!;

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "AircraftAmount")]
    public int NumberInSquad { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "AircraftAmount")]
    public int NumberDuringAttack { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "AircraftAmount")]
    public int MaxNumberOnDeck { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal RestorationTime { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Speed", UnitKey = "Knots")]
    public decimal CruisingSpeed { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Speed", UnitKey = "Knots")]
    public decimal MaxSpeed { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Speed", UnitKey = "Knots")]
    public decimal MinSpeed { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal MaxEngineBoostDuration { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "InitialBoost", UnitKey = "S")]
    public decimal JatoDuration { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "InitialBoost", UnitKey = "PerCent")]
    public decimal JatoSpeedMultiplier { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "HP", UnitKey = "HP")]
    public int PlaneHp { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "HP", UnitKey = "HP")]
    public int SquadronHp { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "HP", UnitKey = "HP")]
    public int AttackGroupHp { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "PerCent")]
    public int DamageTakenDuringAttack { get; set; }

    [DataElementType(DataElementTypes.KeyValue)]
    public int AmmoPerAttack { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "AttackTimings", UnitKey = "S")]
    public decimal PreparationTime { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "AttackTimings", UnitKey = "S")]
    public decimal AimingTime { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "AttackTimings", UnitKey = "S")]
    public decimal TimeToFullyAimed { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "AttackTimings", UnitKey = "S")]
    public decimal PostAttackInvulnerabilityDuration { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "AttackTimings", UnitKey = "S")]
    public decimal AttackCd { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Concealment", UnitKey = "KM")]
    public decimal ConcealmentFromShips { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Concealment", UnitKey = "KM")]
    public decimal ConcealmentFromPlanes { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "KM")]
    public decimal MaxViewDistance { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "AimPenaltyPlanes", UnitKey = "PerCent")]
    public string AimingRateMoving { get; set; } = default!;

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "AimPenaltyPlanes", UnitKey = "PerCent")]
    public string AimingPreparationRateMoving { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "PerCent")]
    public int InnerBombPercentage { get; set; }

    public string WeaponType { get; set; } = default!;

    public ProjectileDataContainer? Weapon { get; set; }

    public List<ConsumableDataContainer> PlaneConsumables { get; set; } = default!;

    // TODO
    public decimal ArmamentReloadTime { get; set; }

    public decimal BoostDurationTime { get; set; }

    public decimal BoostReloadTime { get; set; }

    public static List<CvAircraftDataContainer>? FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<Modifier> modifiers)
    {
        if (ship.CvPlanes.IsEmpty)
        {
            return null;
        }

        var list = new List<CvAircraftDataContainer>();
        var planes = new List<string>();

        var rocketConfiguration = shipConfiguration.Find(c => c.UcType == ComponentType.Fighter);
        if (rocketConfiguration != null)
        {
            var skipModule = ship.CvPlanes[rocketConfiguration.Components[ComponentType.Fighter][0]];
            planes.AddRange(skipModule);
        }

        var torpConfiguration = shipConfiguration.Find(c => c.UcType == ComponentType.TorpedoBomber);
        if (torpConfiguration != null)
        {
            var skipModule = ship.CvPlanes[torpConfiguration.Components[ComponentType.TorpedoBomber][0]];
            planes.AddRange(skipModule);
        }

        var diveConfiguration = shipConfiguration.Find(c => c.UcType == ComponentType.DiveBomber);
        if (diveConfiguration != null)
        {
            var diveModule = ship.CvPlanes[diveConfiguration.Components[ComponentType.DiveBomber][0]];
            planes.AddRange(diveModule);
        }

        var skipConfiguration = shipConfiguration.Find(c => c.UcType == ComponentType.SkipBomber);
        if (skipConfiguration != null)
        {
            var skipModule = ship.CvPlanes[skipConfiguration.Components[ComponentType.SkipBomber][0]];
            planes.AddRange(skipModule);
        }

        foreach (var value in planes)
        {
            int index = value.IndexOf("_", StringComparison.InvariantCultureIgnoreCase);
            string name = value.Substring(0, index);
            var plane = AppData.FindAircraft(name);
            var planeDataContainer = ProcessCvPlane(plane, ship.Tier, modifiers);
            list.Add(planeDataContainer);
        }

        return list;
    }

    private static CvAircraftDataContainer ProcessCvPlane(Aircraft plane, int shipTier, List<Modifier> modifiers)
    {
        int maxOnDeck = modifiers.ApplyModifiers("CvAircraftDataContainer.MaxOnDeck", plane.MaxPlaneInHangar);

        decimal restorationTime = modifiers.ApplyModifiers("CvAircraftDataContainer.RestorationTime", (decimal)plane.RestorationTime);

        decimal planeHp = 0;
        decimal cruisingSpeed = (decimal)plane.Speed;
        decimal minSpeedMultiplier = (decimal)plane.SpeedMinModifier;
        decimal maxSpeedMultiplier = (decimal)plane.SpeedMaxModifier;
        var planesConcealmentFromShips = (decimal)plane.ConcealmentFromShips;
        var planesConcealmentFromPlanes = (decimal)plane.ConcealmentFromPlanes;
        decimal aimRateModifier = 1;
        decimal aimingTime = 0;
        switch (plane.PlaneType)
        {
            case PlaneType.TacticalFighter:
            case PlaneType.Fighter:
                planeHp = modifiers.ApplyModifiers("CvAircraftDataContainer.Hp.Rocket", (decimal)plane.MaxHealth);

                aimingTime = modifiers.ApplyModifiers("CvAircraftDataContainer.AimingTime.Rocket", plane.AimingTime);

                aimRateModifier = modifiers.ApplyModifiers("CvAircraftDataContainer.AimRate.Rocket", aimRateModifier);

                break;
            case PlaneType.TacticalDiveBomber:
            case PlaneType.DiveBomber:
                planeHp = modifiers.ApplyModifiers("CvAircraftDataContainer.Hp.DiveBomber", (decimal)plane.MaxHealth);

                cruisingSpeed = modifiers.ApplyModifiers("CvAircraftDataContainer.CruisingSpeed.DiveBomber", cruisingSpeed);

                minSpeedMultiplier = modifiers.ApplyModifiers("CvAircraftDataContainer.MinSpeed.DiveBomber", minSpeedMultiplier);

                maxSpeedMultiplier = modifiers.ApplyModifiers("CvAircraftDataContainer.MaxSpeed.DiveBomber", maxSpeedMultiplier);

                aimRateModifier = modifiers.ApplyModifiers("CvAircraftDataContainer.AimRate.DiveBomber", aimRateModifier);

                break;
            case PlaneType.TacticalTorpedoBomber:
            case PlaneType.TorpedoBomber:
                planeHp = modifiers.ApplyModifiers("CvAircraftDataContainer.Hp.TorpedoBomber", (decimal)plane.MaxHealth);

                aimingTime = modifiers.ApplyModifiers("CvAircraftDataContainer.AimingTime.TorpedoBomber", plane.AimingTime);

                aimRateModifier = modifiers.ApplyModifiers("CvAircraftDataContainer.AimRate.TorpedoBomber", aimRateModifier);

                break;
            case PlaneType.TacticalSkipBomber:
            case PlaneType.SkipBomber:
                planeHp = modifiers.ApplyModifiers("CvAircraftDataContainer.Hp.SkipBomber", (decimal)plane.MaxHealth);

                cruisingSpeed = modifiers.ApplyModifiers("CvAircraftDataContainer.CruisingSpeed.SkipBomber", cruisingSpeed);

                aimingTime = modifiers.ApplyModifiers("CvAircraftDataContainer.AimingTime.SkipBomber", plane.AimingTime);

                aimRateModifier = modifiers.ApplyModifiers("CvAircraftDataContainer.AimRate.SkipBomber", aimRateModifier);

                break;
        }

        var finalPlaneHp = (int)Math.Round(modifiers.ApplyModifiers("CvAircraftDataContainer.Hp", planeHp));
        var additionalPlaneHp = modifiers.ApplyModifiers("CvAircraftDataContainer.HpPerTier", shipTier);

        // if the value is less than 15, then the modifier was not found and the value is the tier, so we don't add it to hp.
        if (additionalPlaneHp > 15)
        {
            finalPlaneHp += additionalPlaneHp;
        }

        decimal finalCruisingSpeed = modifiers.ApplyModifiers("CvAircraftDataContainer.Speed", cruisingSpeed);

        maxSpeedMultiplier = modifiers.ApplyModifiers("CvAircraftDataContainer.MaxSpeed", maxSpeedMultiplier);

        var maxEngineBoostDuration = modifiers.ApplyModifiers("CvAircraftDataContainer.BoostTime", (decimal)plane.MaxEngineBoostDuration);

        planesConcealmentFromShips = modifiers.ApplyModifiers("CvAircraftDataContainer.Concealment", planesConcealmentFromShips);
        planesConcealmentFromPlanes = modifiers.ApplyModifiers("CvAircraftDataContainer.Concealment", planesConcealmentFromPlanes);

        var jatoDuration = (decimal)plane.JatoData.JatoDuration;
        var jatoMultiplier = (decimal)plane.JatoData.JatoSpeedMultiplier;
        if (jatoDuration == 0)
        {
            jatoMultiplier = 0;
        }

        var weaponType = AppData.FindProjectile(plane.BombName).ProjectileType;
        var bombInnerEllipse = 0;
        ProjectileDataContainer weapon = null!;
        switch (weaponType)
        {
            case ProjectileType.Bomb:
                weapon = BombDataContainer.FromBombName(plane.BombName, modifiers);
                bombInnerEllipse = (int)plane.InnerBombsPercentage;
                break;
            case ProjectileType.SkipBomb:
                weapon = BombDataContainer.FromBombName(plane.BombName, modifiers);
                break;
            case ProjectileType.Torpedo:
                var torpList = new List<string>
                {
                    plane.BombName,
                };
                weapon = TorpedoDataContainer.FromTorpedoName(torpList, modifiers, true)[0];
                break;
            case ProjectileType.Rocket:
                weapon = RocketDataContainer.FromRocketName(plane.BombName, modifiers);
                break;
        }

        List<ConsumableDataContainer> consumables = new();
        foreach (var consumable in plane.AircraftConsumable)
        {
            var consumableDataContainer = ConsumableDataContainer.FromTypeAndVariant(consumable, modifiers, true, 0, ShipClass.AirCarrier);
            consumables.Add(consumableDataContainer);
        }

        consumables = consumables.OrderBy(x => x.Slot).ToList();

        var aimingRateMoving = plane.AimingAccuracyIncreaseRate + plane.AimingAccuracyDecreaseRate;
        var preparationAimingRateMoving = plane.PreparationAccuracyIncreaseRate + plane.PreparationAccuracyDecreaseRate;

        var fullAimTime = plane.PreparationTime + ((1 - (plane.PreparationTime * plane.PreparationAccuracyIncreaseRate * aimRateModifier)) / (plane.AimingAccuracyIncreaseRate * aimRateModifier));

        var jatoSpeedMultiplier = jatoMultiplier > 1 ? (jatoMultiplier - 1) * 100 : 0;

        const string stringFormat = "+#0.0;-#0.0;0";

        var cvAircraft = new CvAircraftDataContainer
        {
            Name = plane.Name,
            PlaneVariant = plane.PlaneType.PlaneTypeToString(),
            PlaneHp = finalPlaneHp,
            SquadronHp = finalPlaneHp * plane.NumPlanesInSquadron,
            AttackGroupHp = finalPlaneHp * plane.AttackData.AttackerSize,
            NumberInSquad = plane.NumPlanesInSquadron,
            MaxNumberOnDeck = maxOnDeck,
            RestorationTime = Math.Round(restorationTime, 2),
            CruisingSpeed = finalCruisingSpeed,
            MaxSpeed = Math.Round(finalCruisingSpeed * maxSpeedMultiplier, 0),
            MinSpeed = Math.Round(finalCruisingSpeed * minSpeedMultiplier, 0),
            MaxEngineBoostDuration = maxEngineBoostDuration,
            InnerBombPercentage = bombInnerEllipse,
            NumberDuringAttack = plane.AttackData.AttackerSize,
            AmmoPerAttack = plane.AttackData.AttackCount,
            AttackCd = Math.Round((decimal)plane.AttackData.AttackCooldown, 1),
            JatoDuration = jatoDuration,
            JatoSpeedMultiplier = Math.Round(jatoSpeedMultiplier, 0),
            WeaponType = weaponType.ProjectileTypeToString(),
            Weapon = weapon,
            PlaneConsumables = consumables,
            AimingTime = Math.Round(aimingTime, 1),
            PreparationTime = plane.PreparationTime,
            PostAttackInvulnerabilityDuration = plane.PostAttackInvulnerabilityDuration,
            DamageTakenDuringAttack = (int)Math.Round(plane.DamageTakenMultiplier * 100),
            AimingRateMoving = (aimingRateMoving * 100).ToString(stringFormat, CultureInfo.InvariantCulture),
            AimingPreparationRateMoving = (preparationAimingRateMoving * 100).ToString(stringFormat, CultureInfo.InvariantCulture),
            TimeToFullyAimed = Math.Round(fullAimTime, 1),
            ConcealmentFromShips = planesConcealmentFromShips,
            ConcealmentFromPlanes = planesConcealmentFromPlanes,
            MaxViewDistance = (decimal)plane.SpottingOnShips,
        };

        cvAircraft.UpdateDataElements();

        return cvAircraft;
    }
}
