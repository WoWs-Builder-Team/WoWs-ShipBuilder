using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataUI.Projectiles;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Translations;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record CVAircraftUI : IDataUi
    {
        [JsonIgnore]
        public string Name { get; set; } = default!;

        [JsonIgnore]
        public int NumberInSquad { get; set; }

        [JsonIgnore]
        public int NumberDuringAttack { get; set; }

        public int AmmoPerAttack { get; set; }

        [JsonIgnore]
        public int MaxNumberOnDeck { get; set; }

        [JsonIgnore]
        public string RestorationTime { get; set; } = default!;

        [JsonIgnore]
        public string PlaneHP { get; set; } = default!;

        [JsonIgnore]
        public string SquadronHP { get; set; } = default!;

        [JsonIgnore]
        public string AttackGroupHP { get; set; } = default!;

        [JsonIgnore]
        public string CruisingSpeed { get; set; } = default!;

        [JsonIgnore]
        public string MaxSpeed { get; set; } = default!;

        [JsonIgnore]
        public string MinSpeed { get; set; } = default!;

        [DataUiUnit("S")]
        public decimal BoostDurationTime { get; set; }

        [DataUiUnit("S")]
        public decimal BoostReloadTime { get; set; }

        [JsonIgnore]
        public string AimingPreparationRateMoving { get; set; } = default!;

        [JsonIgnore]
        public string PreparationTime { get; set; } = default!;

        [JsonIgnore]
        public string AimingTime { get; set; } = default!;

        [JsonIgnore]
        public string AimingRateMoving { get; set; } = default!;

        [JsonIgnore]
        public string PostAttackInvulnerabilityDuration { get; set; } = default!;

        [JsonIgnore]
        public string TimeToFullyAimed { get; set; } = default!;

        [DataUiUnit("PerCent")]
        public int DamageTakenDuringAttack { get; set; }

        [DataUiUnit("S")]
        public decimal ArmamentReloadTime { get; set; }

        [DataUiUnit("PerCent")]
        public int InnerBombPercentage { get; set; }

        [JsonIgnore]
        public string AttackCd { get; set; } = default!;

        [JsonIgnore]
        public string? JatoDuration { get; set; } = default!;

        [JsonIgnore]
        public string JatoSpeedMultiplier { get; set; } = default!;

        [JsonIgnore]
        public string ConcealmentFromShips { get; set; } = default!;

        [JsonIgnore]
        public string ConcealmentFromPlanes { get; set; } = default!;

        [JsonIgnore]
        public string MaxViewDistance { get; set; } = default!;

        [JsonIgnore]
        public string WeaponType { get; set; } = default!;

        [JsonIgnore]
        public bool IsLast { get; set; } = false;

        [JsonIgnore]
        public ProjectileUI? Weapon { get; set; } = default!;

        [JsonIgnore]
        public List<ConsumableUI> PlaneConsumables { get; set; } = default!;

        [JsonIgnore]
        public List<KeyValuePair<string, string>> CVAircraftData { get; set; } = default!;

        public static async Task<List<CVAircraftUI>?> FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string name, float value)> modifiers, IAppDataService appDataService)
        {
            if (ship.CvPlanes is null)
            {
                return null;
            }

            var list = new List<CVAircraftUI>();
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
                var planeUI = await ProcessCVPlane(plane, value.PlaneType, ship.Tier, modifiers, appDataService);
                list.Add(planeUI);
            }

            list.Last().IsLast = true;
            return list;
        }

        private static async Task<CVAircraftUI> ProcessCVPlane(Aircraft plane, PlaneType type, int shipTier, List<(string name, float value)> modifiers, IAppDataService appDataService)
        {
            var maxOnDeckModifiers = modifiers.FindModifiers("planeExtraHangarSize");
            int maxOnDeck = maxOnDeckModifiers.Aggregate(plane.MaxPlaneInHangar, (current, modifier) => (int)(current + modifier));

            var restorationTimeModifiers = modifiers.FindModifiers("planeSpawnTime");
            decimal restorationTime = Math.Round((decimal)restorationTimeModifiers.Aggregate(plane.RestorationTime, (current, modifier) => current * modifier), 2);

            var talentRestorationModifiers = modifiers.FindModifiers("airplaneReloadCoeff");
            restorationTime = Math.Round((decimal)talentRestorationModifiers.Aggregate(restorationTime, (current, modifier) => current * (decimal)modifier), 2);

            float planeHP = 0;
            float cruisingSpeed = plane.Speed;
            float minSpeedMultiplier = plane.SpeedMinModifier;
            float maxSpeedMultiplier = plane.SpeedMaxModifier;
            decimal aimRateModifier = 1;
            decimal aimingTime = 0;
            switch (type)
            {
                case PlaneType.Fighter:
                    var rocketPlaneHPModifiers = modifiers.FindModifiers("fighterHealth");
                    planeHP = (float)Math.Round(rocketPlaneHPModifiers.Aggregate(plane.MaxHealth, (current, modifier) => current * modifier), 0);

                    var rocketAimingTimeModifiers = modifiers.FindModifiers("fighterAimingTime");
                    aimingTime = Math.Round(rocketAimingTimeModifiers.Aggregate(plane.AimingTime, (current, modifier) => current + (decimal)modifier), 1);

                    var aimModifiersRocket = modifiers.FindModifiers("fighterAccuracyIncRateCoeff");
                    aimRateModifier = Math.Round(aimModifiersRocket.Aggregate(aimRateModifier, (current, modifier) => current * (decimal)modifier), 3);

                    break;
                case PlaneType.DiveBomber:
                    var divePlaneHPModifiers = modifiers.FindModifiers("diveBomberHealth");
                    planeHP = (float)Math.Round(divePlaneHPModifiers.Aggregate(plane.MaxHealth, (current, modifier) => current * modifier), 0);

                    var divePlaneSpeedModifiers = modifiers.FindModifiers("diveBomberSpeedMultiplier");
                    cruisingSpeed = (float)Math.Round(divePlaneSpeedModifiers.Aggregate(cruisingSpeed, (current, modifier) => current * modifier), 2);

                    var minSpeedMultiplierModifiers = modifiers.FindModifiers("diveBomberMinSpeedMultiplier");
                    minSpeedMultiplier = minSpeedMultiplierModifiers.Aggregate(minSpeedMultiplier, (current, modifier) => current * modifier);

                    var maxSpeedMultiplierModifiers = modifiers.FindModifiers("diveBomberMaxSpeedMultiplier");
                    maxSpeedMultiplier = maxSpeedMultiplierModifiers.Aggregate(maxSpeedMultiplier, (current, modifier) => current * modifier);

                    var aimModifiersBomber = modifiers.FindModifiers("diveBomberAccuracyIncRateCoeff");
                    aimRateModifier = Math.Round(aimModifiersBomber.Aggregate(aimRateModifier, (current, modifier) => current * (decimal)modifier), 3);

                    break;
                case PlaneType.TorpedoBomber:
                    var torpPlaneHPModifiers = modifiers.FindModifiers("torpedoBomberHealth");
                    planeHP = (float)Math.Round(torpPlaneHPModifiers.Aggregate(plane.MaxHealth, (current, modifier) => current * modifier), 0);

                    var torpAimingTimeModifiers = modifiers.FindModifiers("torpedoBomberAimingTime");
                    aimingTime = Math.Round(torpAimingTimeModifiers.Aggregate(plane.AimingTime, (current, modifier) => current + (decimal)modifier), 1);

                    var aimModifiersTorpedo = modifiers.FindModifiers("torpedoBomberAccuracyIncRateCoeff");
                    aimRateModifier = Math.Round(aimModifiersTorpedo.Aggregate(aimRateModifier, (current, modifier) => current * (decimal)modifier), 3);

                    break;
                case PlaneType.SkipBomber:
                    var skipPlaneHPModifiers = modifiers.FindModifiers("skipBomberHealth");
                    planeHP = (float)Math.Round(skipPlaneHPModifiers.Aggregate(plane.MaxHealth, (current, modifier) => current * modifier), 0);

                    var skipPlaneSpeedModifiers = modifiers.FindModifiers("skipBomberSpeedMultiplier");
                    cruisingSpeed = skipPlaneSpeedModifiers.Aggregate(cruisingSpeed, (current, modifier) => current * modifier);

                    var skipAimingTimeModifiers = modifiers.FindModifiers("skipBomberAimingTime");
                    aimingTime = Math.Round(skipAimingTimeModifiers.Aggregate(plane.AimingTime, (current, modifier) => current * (decimal)modifier), 0);

                    var aimModifiersSkip = modifiers.FindModifiers("skipBomberAccuracyIncRateCoeff");
                    aimRateModifier = Math.Round(aimModifiersSkip.Aggregate(aimRateModifier, (current, modifier) => current * (decimal)modifier), 3);

                    break;
            }

            var allPlaneHpModifiers = modifiers.FindModifiers("planeHealth", true);
            var finalplaneHP = (int)Math.Round(allPlaneHpModifiers.Aggregate(planeHP, (current, modifier) => current * modifier), 2);

            int planeHpPerTierIndex = modifiers.FindModifierIndex("planeHealthPerLevel");
            if (planeHpPerTierIndex > 0)
            {
                int additionalHP = (int)modifiers[planeHpPerTierIndex].value * shipTier;
                finalplaneHP += additionalHP;
            }

            var cruisingSpeedModifiers = modifiers.FindModifiers("planeSpeed");
            decimal finalCruisingSpeed = Math.Round((decimal)cruisingSpeedModifiers.Aggregate(cruisingSpeed, (current, modifier) => current * modifier), 0);

            var talentCruisingSpeedModifiers = modifiers.FindModifiers("squadronSpeedCoeff");
            finalCruisingSpeed = Math.Round(talentCruisingSpeedModifiers.Aggregate(finalCruisingSpeed, (current, modifier) => current * (decimal)modifier), 0);

            var maxSpeedModifiers = modifiers.FindModifiers("planeMaxSpeedMultiplier");
            maxSpeedMultiplier = maxSpeedModifiers.Aggregate(maxSpeedMultiplier, (current, modifier) => current * modifier);

            var jatoDuration = (decimal)plane.JatoData.JatoDuration;
            var jatoMultiplier = (decimal)plane.JatoData.JatoSpeedMultiplier;
            if (jatoDuration == 0)
            {
                jatoMultiplier = 0;
            }

            var weaponType = (await appDataService.GetProjectile(plane.BombName)).ProjectileType;
            var bombInnerEllipse = 0;
            ProjectileUI? weapon = null!;
            switch (weaponType)
            {
                case ProjectileType.Bomb:
                    weapon = await BombUI.FromBombName(plane.BombName, modifiers, appDataService);
                    bombInnerEllipse = (int)plane.InnerBombsPercentage;
                    break;
                case ProjectileType.SkipBomb:
                    weapon = await BombUI.FromBombName(plane.BombName, modifiers, appDataService);
                    break;
                case ProjectileType.Torpedo:
                    var torpList = new List<string>();
                    torpList.Add(plane.BombName);
                    weapon = (await TorpedoUI.FromTorpedoName(torpList, modifiers, true, appDataService)).First();
                    break;
                case ProjectileType.Rocket:
                    weapon = await RocketUI.FromRocketName(plane.BombName, modifiers, appDataService);
                    break;
            }

            List<ConsumableUI> consumables = new();
            foreach (var consumable in plane.AircraftConsumable ?? new List<AircraftConsumable>())
            {
                var consumableUI = ConsumableUI.FromTypeAndVariant(consumable.ConsumableName, consumable.ConsumableVariantName, consumable.Slot, modifiers, true, finalplaneHP, 0);
                consumables.Add(consumableUI);
            }

            consumables = consumables.OrderBy(x => x.Slot).ToList();

            var aimingRateMoving = plane.AimingAccuracyIncreaseRate + plane.AimingAccuracyDecreaseRate;
            var preparationAimingRateMoving = plane.PreparationAccuracyIncreaseRate + plane.PreparationAccuracyDecreaseRate;

            var fullAimTime = plane.PreparationTime + ((1 - (plane.PreparationTime * plane.PreparationAccuracyIncreaseRate * aimRateModifier)) / (plane.AimingAccuracyIncreaseRate * aimRateModifier));

            var jatoSpeedMultiplier = jatoMultiplier > 1 ? Math.Round((jatoMultiplier - 1) * 100, 0) : 0;

            const string stringFormat = "+#0.0 %;-#0.0 %;0 %";

            string? jatoDurationString = jatoDuration != 0 ? $"{jatoDuration} {Translation.Unit_S}" : null;

            var cvAircraft = new CVAircraftUI
            {
                Name = plane.Name,
                PlaneHP = $"{finalplaneHP} {Translation.Unit_HP}",
                SquadronHP = $"{finalplaneHP * plane.NumPlanesInSquadron} {Translation.Unit_HP}",
                AttackGroupHP = $"{finalplaneHP * plane.AttackData.AttackerSize} {Translation.Unit_HP}",
                NumberInSquad = plane.NumPlanesInSquadron,
                MaxNumberOnDeck = maxOnDeck,
                RestorationTime = $"{restorationTime} {Translation.Unit_S}",
                CruisingSpeed = $"{finalCruisingSpeed} {Translation.Unit_Knots}",
                MaxSpeed = $"{Math.Round(finalCruisingSpeed * (decimal)maxSpeedMultiplier, 0)} {Translation.Unit_Knots}",
                MinSpeed = $"{Math.Round(finalCruisingSpeed * (decimal)minSpeedMultiplier, 0)} {Translation.Unit_Knots}",
                InnerBombPercentage = bombInnerEllipse,
                NumberDuringAttack = plane.AttackData.AttackerSize,
                AmmoPerAttack = plane.AttackData.AttackCount,
                AttackCd = $"{Math.Round((decimal)plane.AttackData.AttackCooldown, 1)} {Translation.Unit_S}",
                JatoDuration = jatoDurationString,
                JatoSpeedMultiplier = $"+{jatoSpeedMultiplier} {Translation.Unit_PerCent}",
                WeaponType = weaponType.ToString(),
                Weapon = weapon,
                PlaneConsumables = consumables,
                AimingTime = $"{aimingTime} {Translation.Unit_S}",
                PreparationTime = $"{plane.PreparationTime} {Translation.Unit_S}",
                PostAttackInvulnerabilityDuration = $"{plane.PostAttackInvulnerabilityDuration} {Translation.Unit_S}",
                DamageTakenDuringAttack = (int)Math.Round(plane.DamageTakenMultiplier * 100),
                AimingRateMoving = aimingRateMoving.ToString(stringFormat),
                AimingPreparationRateMoving = preparationAimingRateMoving.ToString(stringFormat),
                TimeToFullyAimed = $"{Math.Round(fullAimTime, 1)} {Translation.Unit_S}",
                ConcealmentFromShips = $"{plane.ConcealmentFromShips} {Translation.Unit_KM}",
                ConcealmentFromPlanes = $"{plane.ConcealmentFromPlanes} {Translation.Unit_KM}",
                MaxViewDistance = $"{plane.SpottingOnShips} {Translation.Unit_KM}",
            };

            cvAircraft.CVAircraftData = cvAircraft.ToPropertyMapping();

            return cvAircraft;
        }
    }
}
