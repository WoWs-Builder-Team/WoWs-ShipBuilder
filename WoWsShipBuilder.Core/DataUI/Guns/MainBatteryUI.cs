using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record MainBatteryUI
    {
        public string Name { get; set; } = default!;

        public decimal Range { get; set; }

        public decimal Reload { get; set; }

        public decimal RoF { get; set; }

        public decimal TurnTime { get; set; }

        public decimal TraverseSpeed { get; set; }

        public decimal Sigma { get; set; }

        public decimal HorizontalDisp { get; set; }

        public decimal VerticalDisp { get; set; }

        public Dispersion DispersionData { get; set; } = default!;

        public static MainBatteryUI? FromShip(Ship ship, IEnumerable<ShipUpgrade> shipConfiguration, List<(string name, float value)> modifiers)
        {
            ShipUpgrade? artilleryConfiguration = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.Artillery);
            if (artilleryConfiguration == null)
            {
                return null;
            }

            TurretModule? mainBattery = ship.MainBatteryModuleList[artilleryConfiguration.Components[ComponentType.Artillery].First()];

            List<(int BarrelCount, int TurretCount)> arrangementList = mainBattery.Guns
                .GroupBy(gun => gun.NumBarrels)
                .Select(group => (BarrelCount: group.Key, TurretCount: group.Count()))
                .OrderBy(item => item.TurretCount)
                .ToList();
            string turretArrangement = string.Join(", ", arrangementList.Select(item => $"{item.TurretCount} x {item.BarrelCount}"));
            Gun gun = mainBattery.Guns.First();

            // Calculate main battery reload
            var reloadModifiers = modifiers
                .Where(modifier => modifier.name.Equals("GMShotDelay", StringComparison.InvariantCultureIgnoreCase))
                .Select(modifier => modifier.value);
            decimal reload = Math.Round(reloadModifiers.Aggregate(gun.Reload, (current, reloadModifier) => current * (decimal)reloadModifier), 2);

            // Rotation speed modifiers
            var turnSpeedModifiers = modifiers
                .Where(modifier => modifier.name.Equals("GMRotationSpeed", StringComparison.InvariantCultureIgnoreCase))
                .Select(modifier => modifier.value);
            decimal traverseSpeed =
                Math.Round(turnSpeedModifiers.Aggregate(gun.HorizontalRotationSpeed, (current, modifier) => current * (decimal)modifier), 2);

            // Range modifiers
            var rangeModifiers = modifiers
                .Where(modifier => modifier.name.Equals("GMMaxDist", StringComparison.InvariantCultureIgnoreCase))
                .Select(modifier => modifier.value);
            decimal range = Math.Round(rangeModifiers.Aggregate(mainBattery.MaxRange, (current, modifier) => current * (decimal)modifier) / 1000, 2);

            // Adjust dispersion for range modifiers
            decimal hDispersion = Math.Round((decimal)mainBattery.DispersionValues.CalculateHorizontalDispersion((double)mainBattery.MaxRange), 2);
            decimal vDispersion = Math.Round((decimal)mainBattery.DispersionValues.CalculateVerticalDispersion((double)mainBattery.MaxRange), 2);

            return new MainBatteryUI
            {
                Name = turretArrangement + " " + Localizer.Instance[mainBattery.Guns.First().Name].Localization,
                Range = range,
                Reload = reload,
                RoF = Math.Round(60 / reload),
                TurnTime = Math.Round(180 / traverseSpeed, 1),
                TraverseSpeed = traverseSpeed,
                Sigma = mainBattery.Sigma,
                HorizontalDisp = hDispersion,
                VerticalDisp = vDispersion,
                DispersionData = mainBattery.DispersionValues,
            };
        }
    }
}
