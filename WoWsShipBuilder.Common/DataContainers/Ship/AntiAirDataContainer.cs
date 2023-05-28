using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Infrastructure.Utility;

// ReSharper disable InconsistentNaming
namespace WoWsShipBuilder.DataContainers;

public record AntiAirDataContainer
{
    // These two are for the conversion in damage per second
    private const decimal ConstantDamageMultiplier = 1 / AntiAirAura.DamageInterval;

    private const decimal FlakDamageMultiplier = 1 / (AntiAirAura.DamageInterval / 2);

    private string expanderKey = default!;

    public bool IsExpanderOpen
    {
        get => ShipDataContainer.ExpanderStateMapper[expanderKey];
        set => ShipDataContainer.ExpanderStateMapper[expanderKey] = value;
    }

    public AuraDataDataContainer? LongRangeAura { get; set; }

    public AuraDataDataContainer? MediumRangeAura { get; set; }

    public AuraDataDataContainer? ShortRangeAura { get; set; }

    public static AntiAirDataContainer? FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string Name, float Value)> modifiers)
    {
        if (ship.ShipClass.Equals(ShipClass.Submarine))
        {
            return null;
        }

        var hull = ship.Hulls[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Hull).Components[ComponentType.Hull].First()];
        TurretModule? guns = null;
        if (ship.MainBatteryModuleList is { Count: > 0 })
        {
            guns = ship.MainBatteryModuleList[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Artillery).Components[ComponentType.Artillery].First()];
        }

        decimal flakDamageBonus = modifiers.FindModifiers("AABubbleDamage").Aggregate(1M, (current, value) => current * (decimal)value);
        flakDamageBonus = modifiers.FindModifiers("bubbleDamageMultiplier").Aggregate(flakDamageBonus, (current, modifier) => current * (decimal)modifier);

        decimal constantDamageBonus = modifiers.FindModifiers("AAAuraDamage").Aggregate(1M, (current, value) => current * (decimal)value);
        constantDamageBonus = modifiers.FindModifiers("areaDamageMultiplier").Aggregate(constantDamageBonus, (current, value) => current * (decimal)value);

        IEnumerable<float> constantDamageBonusModifiers = modifiers.FindModifiers("lastChanceReloadCoefficient");
        constantDamageBonus = constantDamageBonusModifiers.Aggregate(constantDamageBonus, (current, arModifier) => current * (1 + ((decimal)arModifier / 100)));

        var aaUI = new AntiAirDataContainer
        {
            expanderKey = $"{ship.Index}_AA",
        };
        if (!ShipDataContainer.ExpanderStateMapper.ContainsKey(aaUI.expanderKey))
        {
            ShipDataContainer.ExpanderStateMapper[aaUI.expanderKey] = true;
        }

        // Long Range Aura
        AntiAirAura? longRange = null;
        if (hull.AntiAir?.LongRangeAura != null)
        {
            longRange = hull.AntiAir.LongRangeAura;
        }

        if (guns?.AntiAir != null)
        {
            if (longRange is null)
            {
                longRange = guns.AntiAir;
            }
            else
            {
                longRange = longRange.AddAura(guns.AntiAir);
            }
        }

        var flakAmount = 0;
        if (longRange != null)
        {
            IEnumerable<float> extraFlak = modifiers.FindModifiers("AAExtraBubbles"); // extra flak explosions from captain skill
            IEnumerable<float> extraFlakInner = modifiers.FindModifiers("AAInnerExtraBubbles"); // extra flak explosions from auxiliary armament mod 2
            flakAmount = extraFlak.Concat(extraFlakInner).Aggregate(longRange.FlakCloudsNumber, (current, modifier) => current + (int)modifier);
        }

        aaUI.LongRangeAura = FromAura(longRange, flakDamageBonus, constantDamageBonus, flakAmount);
        aaUI.MediumRangeAura = FromAura(hull.AntiAir?.MediumRangeAura, flakDamageBonus, constantDamageBonus, 0);
        aaUI.ShortRangeAura = FromAura(hull.AntiAir?.ShortRangeAura, flakDamageBonus, constantDamageBonus, 0);

        if (aaUI.ShortRangeAura == null && aaUI.MediumRangeAura == null && aaUI.LongRangeAura == null)
        {
            return null;
        }

        return aaUI;
    }

    private static AuraDataDataContainer? FromAura(AntiAirAura? antiAirAura, decimal flakDamageBonus, decimal constantDamageBonus, int flakAmount)
    {
        if (antiAirAura == null)
        {
            return null;
        }

        var flakNumber = "";
        if (flakAmount > 0)
        {
            var flakAverage = (int)(flakAmount * antiAirAura.HitChance);
            int flakDelta = flakAmount - flakAverage;
            flakNumber = $"{flakAverage} ± {flakDelta}";
        }

        var auraData = new AuraDataDataContainer
        {
            Range = Math.Round(antiAirAura.MaxRange / 1000, 2),
            ConstantDamage = Math.Round(antiAirAura.ConstantDps * ConstantDamageMultiplier * constantDamageBonus, 2),
            Flak = flakNumber,
            FlakDamage = Math.Round(antiAirAura.FlakDamage * FlakDamageMultiplier * flakDamageBonus, 2),
            HitChance = (int)Math.Round(antiAirAura.HitChance * 100, 2),
        };

        auraData.UpdateData();
        return auraData;
    }
}
