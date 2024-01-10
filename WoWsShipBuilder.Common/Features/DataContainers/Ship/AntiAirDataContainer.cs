using System.Collections.Immutable;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;

// ReSharper disable InconsistentNaming
namespace WoWsShipBuilder.Features.DataContainers;

public class AntiAirDataContainer
{
    // These two are for the conversion in damage per second
    private const decimal ConstantDamageMultiplier = 1 / AntiAirAura.DamageInterval;

    private const decimal FlakDamageMultiplier = 1 / (AntiAirAura.DamageInterval / 2);

    public AuraDataDataContainer? LongRangeAura { get; set; }

    public AuraDataDataContainer? MediumRangeAura { get; set; }

    public AuraDataDataContainer? ShortRangeAura { get; set; }

    public static AntiAirDataContainer? FromShip(Ship ship, ImmutableList<ShipUpgrade> shipConfiguration, List<Modifier> modifiers)
    {
        if (ship.ShipClass.Equals(ShipClass.Submarine))
        {
            return null;
        }

        var hull = ship.Hulls[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Hull).Components[ComponentType.Hull][0]];
        TurretModule? guns = null;
        if (ship.MainBatteryModuleList is { Count: > 0 })
        {
            guns = ship.MainBatteryModuleList[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Artillery).Components[ComponentType.Artillery][0]];
        }

        decimal flakDamageBonus = modifiers.ApplyModifiers("AntiAirDataContainer.FlakDamage", 1M);

        decimal constantDamageBonus = modifiers.ApplyModifiers("AntiAirDataContainer.ConstantDamage", 1M);

        var aaUI = new AntiAirDataContainer();

        // Long Range Aura
        AntiAirAura? longRange = null;
        if (hull.AntiAir?.LongRangeAura != null)
        {
            longRange = hull.AntiAir.LongRangeAura;
        }

        if (guns?.AntiAir != null)
        {
            longRange = longRange is null ? guns.AntiAir : longRange.AddAura(guns.AntiAir);
        }

        var flakAmount = 0;
        if (longRange != null)
        {
            flakAmount = modifiers.ApplyModifiers("AntiAirDataContainer.FlakAmount", longRange.FlakCloudsNumber);
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
