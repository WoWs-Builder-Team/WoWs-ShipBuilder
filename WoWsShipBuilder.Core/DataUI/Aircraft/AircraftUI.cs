using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI.Aircrafts
{
    public record AircraftUI
    {
        public string Name { get; set; } = default!;

        public int NumberInSquad { get; set; }

        public int NumberDuringAttack { get; set; }

        public int AmmoPerAttack { get; set; }

        public int StartingNumberOnDeck { get; set; }

        public int MaxNumberOnDeck { get; set; }

        public decimal RestorationTime { get; set; }

        public int PlaneHP { get; set; }

        public decimal CruisingSpeed { get; set; }

        public decimal MaxSpeed { get; set; }

        public decimal MinSpeed { get; set; }

        public decimal BoostDurationTime { get; set; }

        public decimal BoostReloadTime { get; set; }

        public decimal ArmamentReloadTime { get; set; }

        public static AircraftUI? FromAircraftName(string name, int shipTier, PlaneCategory category, PlaneType type, List<(string name, float value)> modifiers)
        {
            var plane = AppData.AircraftList![name];

            AircraftUI aircraft = null!;

            if (category.Equals(PlaneCategory.Cv))
            {
                aircraft = ProcessCVPlane(plane, type, shipTier,  modifiers);
            }

            return aircraft;
        }

        public static AircraftUI ProcessCVPlane(Aircraft plane, PlaneType type, int shipTier, List<(string name, float value)> modifiers)
        {
            var maxOnDeckModifiers = modifiers.FindModifiers("planeExtraHangarSize");
            int maxOnDeck = maxOnDeckModifiers.Aggregate(plane.MaxPlaneInHangar, (current, modifier) => (int)(current + modifier));

            var restorationTimeModifiers = modifiers.FindModifiers("planeSpawnTime");
            decimal restorationTime = Math.Round((decimal)restorationTimeModifiers.Aggregate(plane.RestorationTime, (current, modifier) => current * modifier), 2);

            float planeHP = 0;
            float cruisingSpeed = plane.Speed;
            float minSpeedMultiplier = plane.SpeedMinModifier;
            float maxSpeedMultiplier = plane.SpeedMaxModifier;
            switch (type)
            {
                case PlaneType.Fighter:
                    var rocketPlaneHPModifiers = modifiers.FindModifiers("fighterHealth");
                    planeHP = (float)Math.Round(rocketPlaneHPModifiers.Aggregate(plane.MaxHealth, (current, modifier) => current * modifier), 0);

                    break;
                case PlaneType.DiveBomber:
                    var divePlaneHPModifiers = modifiers.FindModifiers("diveBomberHealth");
                    planeHP = (float)Math.Round(divePlaneHPModifiers.Aggregate(plane.MaxHealth, (current, modifier) => current * modifier), 0);

                    var divePlaneSpeedModifiers = modifiers.FindModifiers("diveBomberSpeedMultiplier");
                    cruisingSpeed = (float)Math.Round(divePlaneSpeedModifiers.Aggregate(cruisingSpeed, (current, modifier) => current * modifier), 2);

                    var minSpeedMultiplierModifiers = modifiers.FindModifiers("diveBomberMinSpeedMultiplier");
                    minSpeedMultiplier = (float)Math.Round(minSpeedMultiplierModifiers.Aggregate(minSpeedMultiplier, (current, modifier) => current * modifier), 0);

                    var maxSpeedMultiplierModifiers = modifiers.FindModifiers("diveBomberMaxSpeedMultiplier");
                    maxSpeedMultiplier = (float)Math.Round(maxSpeedMultiplierModifiers.Aggregate(maxSpeedMultiplier, (current, modifier) => current * modifier), 0);

                    break;
                case PlaneType.TorpedoBomber:
                    var torpPlaneHPModifiers = modifiers.FindModifiers("torpedoBomberHealth");
                    planeHP = (float)Math.Round(torpPlaneHPModifiers.Aggregate(plane.MaxHealth, (current, modifier) => current * modifier), 0);

                    break;
                case PlaneType.SkipBomber:
                    var skipPlaneHPModifiers = modifiers.FindModifiers("skipBomberHealth");
                    planeHP = (float)Math.Round(skipPlaneHPModifiers.Aggregate(plane.MaxHealth, (current, modifier) => current * modifier), 0);

                    var skipPlaneSpeedModifiers = modifiers.FindModifiers("skipBomberSpeedMultiplier");
                    cruisingSpeed = (float)Math.Round(skipPlaneSpeedModifiers.Aggregate(cruisingSpeed, (current, modifier) => current * modifier), 2);

                    break;
            }

            var allPlaneHpModifiers = modifiers.FindModifiers("planeHealth");
            int finalplaneHP = (int)Math.Round(allPlaneHpModifiers.Aggregate(planeHP, (current, modifier) => current * modifier), 2);

            var planeHpPerTierIndex = modifiers.FindModifierIndex("planeHealthPerLevel");
            if (planeHpPerTierIndex > 0)
            {
                var additionalHP = (int)modifiers[planeHpPerTierIndex].value * shipTier;
                finalplaneHP += additionalHP;
            }

            var cruisingSpeedModifiers = modifiers.FindModifiers("planeSpeed");
            decimal finalCruisingSpeed = Math.Round((decimal)restorationTimeModifiers.Aggregate(cruisingSpeed, (current, modifier) => current * modifier), 2);

            return new AircraftUI
            {
                Name = plane.Name,
                NumberInSquad = plane.NumPlanesInSquadron,
                NumberDuringAttack = plane.AttackData.AttackerSize,
                AmmoPerAttack = plane.AttackData.AttackCount,
                StartingNumberOnDeck = plane.StartingPlanes,
                MaxNumberOnDeck = maxOnDeck,
                RestorationTime = restorationTime,
                PlaneHP = finalplaneHP,
                CruisingSpeed = finalCruisingSpeed,
                MaxSpeed = finalCruisingSpeed * (decimal)maxSpeedMultiplier,
                MinSpeed = finalCruisingSpeed * (decimal)minSpeedMultiplier,
            };
        }
    }
}
