using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataUI.Projectiles;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record AirstrikeUI : IDataUi
    {
        private string expanderKey = default!;

        [JsonIgnore]
        public bool IsExpanderOpen
        {
            get => ShipUI.ExpanderStateMapper[expanderKey];
            set => ShipUI.ExpanderStateMapper[expanderKey] = value;
        }

        [JsonIgnore]
        public string Name { get; set; } = default!;

        [JsonIgnore]
        public string Header { get; set; } = default!;

        [DataUiUnit("HP")]
        public int PlaneHP { get; set; }

        public int NumberOfUses { get; set; }

        [DataUiUnit("S")]
        public decimal FlyAwayTime { get; set; }

        [DataUiUnit("KM")]
        public decimal MinimumDistance { get; set; }

        [DataUiUnit("KM")]
        public decimal MaximumDistance { get; set; }

        [DataUiUnit("KM")]
        public decimal MaximumFlightDistance { get; set; }

        [DataUiUnit("S")]
        public decimal Reload { get; set; }

        [DataUiUnit("S")]
        public decimal DropTime { get; set; }

        public int NumberDuringAttack { get; set; }

        public int BombsPerPlane { get; set; }

        [JsonIgnore]
        public string WeaponType { get; set; } = null!;

        [JsonIgnore]
        public ProjectileUI? Weapon { get; set; } = default!;

        [JsonIgnore]
        public List<KeyValuePair<string, string>> AirstrikeData { get; set; } = default!;

        public static AirstrikeUI? FromShip(Ship ship, List<(string, float)> modifiers, bool isAsw)
        {
            var header = isAsw ? "ShipStats_AswAirstrike" : "ShipStats_Airstrike";
            var airstrikes = ship.AirStrikes;
            if (ship.AirStrikes == null || ship.AirStrikes.Count == 0)
            {
                return null;
            }

            var airstrike = airstrikes.FirstOrDefault().Value;
            var plane = AppDataHelper.Instance.FindAswAircraft(airstrike.PlaneName.Substring(0, airstrike.PlaneName.IndexOf("_", StringComparison.InvariantCultureIgnoreCase)));

            if (isAsw != plane.PlaneCategory.Equals(PlaneCategory.Asw))
            {
                return null;
            }

            var reloadModifiers = modifiers.FindModifiers("asReloadTimeCoeff");
            var reload = Math.Round(reloadModifiers.Aggregate(airstrike.ReloadTime, (current, modifier) => current * (decimal)modifier), 2);

            var planeHp = plane.MaxHealth;
            var planeHpModifiers = modifiers.FindModifiers("asMaxHealthCoeff");
            var finalPlaneHp = (int)Math.Round(planeHpModifiers.Aggregate(planeHp, (current, modifier) => current * modifier), 0);

            ProjectileUI? weapon = null!;
            string weaponType;
            if (isAsw)
            {
                weapon = DepthChargeUI.FromChargesName(plane.BombName, modifiers);
                weaponType = ProjectileType.DepthCharge.ToString();
            }
            else
            {
                weapon = BombUI.FromBombName(plane.BombName, modifiers);
                weaponType = ProjectileType.Bomb.ToString();
            }

            var airstrikeUI = new AirstrikeUI
            {
                Header = header,
                Name = airstrike.PlaneName,
                PlaneHP = finalPlaneHp,
                Reload = reload,
                NumberOfUses = airstrike.Charges,
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

            airstrikeUI.expanderKey = isAsw ? $"{ship.Index}_ASW" : $"{ship.Index}_AS";
            airstrikeUI.AirstrikeData = airstrikeUI.ToPropertyMapping();

            if (!ShipUI.ExpanderStateMapper.ContainsKey(airstrikeUI.expanderKey))
            {
                ShipUI.ExpanderStateMapper[airstrikeUI.expanderKey] = true;
            }

            return airstrikeUI;
        }
    }
}
