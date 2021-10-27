using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record SecondaryBatteryUI : IDataUi
    {
        public string Name { get; set; } = default!;

        public decimal Range { get; set; }

        public decimal Damage { get; set; }

        public decimal Penetration { get; set; }

        public decimal? FireChance { get; set; }

        public decimal Reload { get; set; }

        public decimal RoF { get; set; }

        public decimal Sigma { get; set; }

        public decimal HorizontalDisp { get; set; }

        public decimal VerticalDisp { get; set; }

        public decimal ShellVelocity { get; set; }

        public static List<SecondaryBatteryUI>? FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers)
        {
            TurretModule? secondary = ship.Hulls[shipConfiguration.First(c => c.UcType == ComponentType.Hull).Components[ComponentType.Hull].First()]
                .SecondaryModule;
            if (secondary == null)
            {
                return null;
            }

            var result = new List<SecondaryBatteryUI>();
            List<List<Gun>> groupedSecondaries = secondary.Guns.GroupBy(gun => new { gun.BarrelDiameter, gun.NumBarrels })
                .OrderBy(group => group.Key.BarrelDiameter)
                .ThenBy(group => group.Key.NumBarrels)
                .Select(group => group.ToList())
                .ToList();

            foreach (List<Gun> secondaryGroup in groupedSecondaries)
            {
                var secondaryGun = secondaryGroup.First();
                var ui = new SecondaryBatteryUI
                {
                    Name = $"{secondaryGroup.Count} x {secondaryGun.NumBarrels}" + Localizer.Instance[secondaryGun.Name].Localization,
                    Range = secondary.MaxRange,
                };

                // result.Add(ui);
            }

            return result;
        }
    }
}
