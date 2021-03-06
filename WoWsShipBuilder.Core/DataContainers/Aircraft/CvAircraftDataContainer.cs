using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataElements.DataElements;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataContainers
{
    public partial record CvAircraftDataContainer : DataContainerBase
    {
        public string Name { get; set; } = default!;

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

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "InitialBoost", UnitKey = "S")]
        public decimal JatoDuration { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "InitialBoost", UnitKey = "PerCent")]
        public decimal JatoSpeedMultiplier { get; set; }

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

        public bool IsLast { get; set; }

        public ProjectileDataContainer? Weapon { get; set; }

        public List<ConsumableDataContainer> PlaneConsumables { get; set; } = default!;

        // TODO
        public decimal ArmamentReloadTime { get; set; }

        public decimal BoostDurationTime { get; set; }

        public decimal BoostReloadTime { get; set; }

        public static async Task<List<CvAircraftDataContainer>?> FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string name, float value)> modifiers, IAppDataService appDataService)
        {
            if (ship.CvPlanes is null)
            {
                return null;
            }

            var list = new List<CvAircraftDataContainer>();
            var planes = new List<PlaneData>();

            var rocketConfiguration = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.Fighter);
            if (rocketConfiguration != null)
            {
                List<PlaneData>? skipModule = ship.CvPlanes[rocketConfiguration.Components[ComponentType.Fighter].First()];
                planes.AddRange(skipModule);
            }

            var torpConfiguration = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.TorpedoBomber);
            if (torpConfiguration != null)
            {
                List<PlaneData>? skipModule = ship.CvPlanes[torpConfiguration.Components[ComponentType.TorpedoBomber].First()];
                planes.AddRange(skipModule);
            }

            var diveConfiguration = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.DiveBomber);
            if (diveConfiguration != null)
            {
                List<PlaneData>? diveModule = ship.CvPlanes[diveConfiguration.Components[ComponentType.DiveBomber].First()];
                planes.AddRange(diveModule);
            }

            var skipConfiguration = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.SkipBomber);
            if (skipConfiguration != null)
            {
                List<PlaneData>? skipModule = ship.CvPlanes[skipConfiguration.Components[ComponentType.SkipBomber].First()];
                planes.AddRange(skipModule);
            }

            foreach (var value in planes)
            {
                int index = value.PlaneName.IndexOf("_", StringComparison.InvariantCultureIgnoreCase);
                string name = value.PlaneName.Substring(0, index);
                var plane = await appDataService.GetAircraft(name);
                var planeDataContainer = await ProcessCvPlane(plane, value.PlaneType, ship.Tier, modifiers, appDataService);
                list.Add(planeDataContainer);
            }

            list.Last().IsLast = true;
            return list;
        }

        private static async Task<CvAircraftDataContainer> ProcessCvPlane(Aircraft plane, PlaneType type, int shipTier, List<(string name, float value)> modifiers, IAppDataService appDataService)
        {
            var maxOnDeckModifiers = modifiers.FindModifiers("planeExtraHangarSize");
            int maxOnDeck = maxOnDeckModifiers.Aggregate(plane.MaxPlaneInHangar, (current, modifier) => (int)(current + modifier));

            var restorationTimeModifiers = modifiers.FindModifiers("planeSpawnTime");
            decimal restorationTime = (decimal)restorationTimeModifiers.Aggregate(plane.RestorationTime, (current, modifier) => current * modifier);

            var talentRestorationModifiers = modifiers.FindModifiers("airplaneReloadCoeff");
            restorationTime = talentRestorationModifiers.Aggregate(restorationTime, (current, modifier) => current * (decimal)modifier);

            float planeHp = 0;
            float cruisingSpeed = plane.Speed;
            float minSpeedMultiplier = plane.SpeedMinModifier;
            float maxSpeedMultiplier = plane.SpeedMaxModifier;
            var planesConcealmentFromShips = (float)plane.ConcealmentFromShips;
            var planesConcealmentFromPlanes = (float)plane.ConcealmentFromPlanes;
            decimal aimRateModifier = 1;
            decimal aimingTime = 0;
            switch (type)
            {
                case PlaneType.Fighter:
                    var rocketPlaneHpModifiers = modifiers.FindModifiers("fighterHealth");
                    planeHp = rocketPlaneHpModifiers.Aggregate(plane.MaxHealth, (current, modifier) => current * modifier);

                    var rocketAimingTimeModifiers = modifiers.FindModifiers("fighterAimingTime");
                    aimingTime = rocketAimingTimeModifiers.Aggregate(plane.AimingTime, (current, modifier) => current + (decimal)modifier);

                    var aimModifiersRocket = modifiers.FindModifiers("fighterAccuracyIncRateCoeff");
                    aimRateModifier = aimModifiersRocket.Aggregate(aimRateModifier, (current, modifier) => current * (decimal)modifier);

                    break;
                case PlaneType.DiveBomber:
                    var divePlaneHpModifiers = modifiers.FindModifiers("diveBomberHealth");
                    planeHp = divePlaneHpModifiers.Aggregate(plane.MaxHealth, (current, modifier) => current * modifier);

                    var divePlaneSpeedModifiers = modifiers.FindModifiers("diveBomberSpeedMultiplier");
                    cruisingSpeed = divePlaneSpeedModifiers.Aggregate(cruisingSpeed, (current, modifier) => current * modifier);

                    var minSpeedMultiplierModifiers = modifiers.FindModifiers("diveBomberMinSpeedMultiplier");
                    minSpeedMultiplier = minSpeedMultiplierModifiers.Aggregate(minSpeedMultiplier, (current, modifier) => current * modifier);

                    var maxSpeedMultiplierModifiers = modifiers.FindModifiers("diveBomberMaxSpeedMultiplier");
                    maxSpeedMultiplier = maxSpeedMultiplierModifiers.Aggregate(maxSpeedMultiplier, (current, modifier) => current * modifier);

                    var aimModifiersBomber = modifiers.FindModifiers("diveBomberAccuracyIncRateCoeff");
                    aimRateModifier = aimModifiersBomber.Aggregate(aimRateModifier, (current, modifier) => current * (decimal)modifier);

                    break;
                case PlaneType.TorpedoBomber:
                    var torpPlaneHpModifiers = modifiers.FindModifiers("torpedoBomberHealth");
                    planeHp = torpPlaneHpModifiers.Aggregate(plane.MaxHealth, (current, modifier) => current * modifier);

                    var torpAimingTimeModifiers = modifiers.FindModifiers("torpedoBomberAimingTime");
                    aimingTime = torpAimingTimeModifiers.Aggregate(plane.AimingTime, (current, modifier) => current + (decimal)modifier);

                    var aimModifiersTorpedo = modifiers.FindModifiers("torpedoBomberAccuracyIncRateCoeff");
                    aimRateModifier = aimModifiersTorpedo.Aggregate(aimRateModifier, (current, modifier) => current * (decimal)modifier);

                    break;
                case PlaneType.SkipBomber:
                    var skipPlaneHpModifiers = modifiers.FindModifiers("skipBomberHealth");
                    planeHp = skipPlaneHpModifiers.Aggregate(plane.MaxHealth, (current, modifier) => current * modifier);

                    var skipPlaneSpeedModifiers = modifiers.FindModifiers("skipBomberSpeedMultiplier");
                    cruisingSpeed = skipPlaneSpeedModifiers.Aggregate(cruisingSpeed, (current, modifier) => current * modifier);

                    var skipAimingTimeModifiers = modifiers.FindModifiers("skipBomberAimingTime");
                    aimingTime = skipAimingTimeModifiers.Aggregate(plane.AimingTime, (current, modifier) => current * (decimal)modifier);

                    var aimModifiersSkip = modifiers.FindModifiers("skipBomberAccuracyIncRateCoeff");
                    aimRateModifier = aimModifiersSkip.Aggregate(aimRateModifier, (current, modifier) => current * (decimal)modifier);

                    break;
            }

            var allPlaneHpModifiers = modifiers.FindModifiers("planeHealth", true);
            var finalPlaneHp = (int)Math.Round(allPlaneHpModifiers.Aggregate(planeHp, (current, modifier) => current * modifier), 0);

            int planeHpPerTierIndex = modifiers.FindModifierIndex("planeHealthPerLevel");
            if (planeHpPerTierIndex > 0)
            {
                int additionalHp = (int)modifiers[planeHpPerTierIndex].value * shipTier;
                finalPlaneHp += additionalHp;
            }

            var cruisingSpeedModifiers = modifiers.FindModifiers("planeSpeed");
            decimal finalCruisingSpeed = (decimal)cruisingSpeedModifiers.Aggregate(cruisingSpeed, (current, modifier) => current * modifier);

            var talentCruisingSpeedModifiers = modifiers.FindModifiers("squadronSpeedCoeff");
            finalCruisingSpeed = talentCruisingSpeedModifiers.Aggregate(finalCruisingSpeed, (current, modifier) => current * (decimal)modifier);

            var maxSpeedModifiers = modifiers.FindModifiers("planeMaxSpeedMultiplier");
            maxSpeedMultiplier = maxSpeedModifiers.Aggregate(maxSpeedMultiplier, (current, modifier) => current * modifier);

            var planesConcealmentModifiers = modifiers.FindModifiers("planeVisibilityFactor").ToList();
            planesConcealmentFromShips = planesConcealmentModifiers.Aggregate(planesConcealmentFromShips, (current, modifier) => current * modifier);
            planesConcealmentFromPlanes = planesConcealmentModifiers.Aggregate(planesConcealmentFromPlanes, (current, modifier) => current * modifier);

            var jatoDuration = (decimal)plane.JatoData.JatoDuration;
            var jatoMultiplier = (decimal)plane.JatoData.JatoSpeedMultiplier;
            if (jatoDuration == 0)
            {
                jatoMultiplier = 0;
            }

            var weaponType = (await appDataService.GetProjectile(plane.BombName)).ProjectileType;
            var bombInnerEllipse = 0;
            ProjectileDataContainer weapon = null!;
            switch (weaponType)
            {
                case ProjectileType.Bomb:
                    weapon = await BombDataContainer.FromBombName(plane.BombName, modifiers, appDataService);
                    bombInnerEllipse = (int)plane.InnerBombsPercentage;
                    break;
                case ProjectileType.SkipBomb:
                    weapon = await BombDataContainer.FromBombName(plane.BombName, modifiers, appDataService);
                    break;
                case ProjectileType.Torpedo:
                    var torpList = new List<string>
                    {
                        plane.BombName,
                    };
                    weapon = (await TorpedoDataContainer.FromTorpedoName(torpList, modifiers, true, appDataService)).First();
                    break;
                case ProjectileType.Rocket:
                    weapon = await RocketDataContainer.FromRocketName(plane.BombName, modifiers, appDataService);
                    break;
            }

            List<ConsumableDataContainer> consumables = new();
            foreach (var consumable in plane.AircraftConsumable ?? new List<AircraftConsumable>())
            {
                var consumableDataContainer = await ConsumableDataContainer.FromTypeAndVariant(consumable, modifiers, true, finalPlaneHp, 0, appDataService);
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
                PlaneHp = finalPlaneHp,
                SquadronHp = finalPlaneHp * plane.NumPlanesInSquadron,
                AttackGroupHp = finalPlaneHp * plane.AttackData.AttackerSize,
                NumberInSquad = plane.NumPlanesInSquadron,
                MaxNumberOnDeck = maxOnDeck,
                RestorationTime = Math.Round(restorationTime, 2),
                CruisingSpeed = finalCruisingSpeed,
                MaxSpeed = Math.Round(finalCruisingSpeed * (decimal)maxSpeedMultiplier, 0),
                MinSpeed = Math.Round(finalCruisingSpeed * (decimal)minSpeedMultiplier, 0),
                InnerBombPercentage = bombInnerEllipse,
                NumberDuringAttack = plane.AttackData.AttackerSize,
                AmmoPerAttack = plane.AttackData.AttackCount,
                AttackCd = Math.Round((decimal)plane.AttackData.AttackCooldown, 1),
                JatoDuration = jatoDuration,
                JatoSpeedMultiplier = Math.Round(jatoSpeedMultiplier, 0),
                WeaponType = weaponType.ToString(),
                Weapon = weapon,
                PlaneConsumables = consumables,
                AimingTime = Math.Round(aimingTime, 1),
                PreparationTime = plane.PreparationTime,
                PostAttackInvulnerabilityDuration = plane.PostAttackInvulnerabilityDuration,
                DamageTakenDuringAttack = (int)Math.Round(plane.DamageTakenMultiplier * 100),
                AimingRateMoving = aimingRateMoving.ToString(stringFormat),
                AimingPreparationRateMoving = preparationAimingRateMoving.ToString(stringFormat),
                TimeToFullyAimed = Math.Round(fullAimTime, 1),
                ConcealmentFromShips = (decimal)planesConcealmentFromShips,
                ConcealmentFromPlanes = (decimal)planesConcealmentFromPlanes,
                MaxViewDistance = (decimal)plane.SpottingOnShips,
            };

            cvAircraft.UpdateDataElements();

            return cvAircraft;
        }
    }
}
