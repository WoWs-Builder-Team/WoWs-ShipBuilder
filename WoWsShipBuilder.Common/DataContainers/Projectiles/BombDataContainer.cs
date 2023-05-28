using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures.Projectile;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.GameData;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.DataContainers;

public partial record BombDataContainer : ProjectileDataContainer
{
    [DataElementType(DataElementTypes.KeyValue, IsValueLocalizationKey = true, IsValueAppLocalization = true)]
    public string BombType { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValue, IsValueLocalizationKey = true)]
    public string Name { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValue)]
    public decimal Damage { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = "Splash", TooltipKey = "SplashExplanation", UnitKey = "M")]
    public decimal SplashRadius { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = "Splash", TooltipKey = "SplashExplanation")]
    public decimal SplashDmg { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "MM")]
    public int Penetration { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal FuseTimer { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "MM")]
    public int ArmingThreshold { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Degree")]
    public string RicochetAngles { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "PerCent")]
    public decimal FireChance { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Blast", UnitKey = "M")]
    public decimal ExplosionRadius { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = "Blast", TooltipKey = "BlastExplanation")]
    [DataElementFiltering(true, "ShouldDisplayBlastPenetration")]
    public decimal SplashCoeff { get; set; }

    public bool ShowBlastPenetration { get; private set; }

    public static BombDataContainer FromBombName(string name, List<(string name, float value)> modifiers)
    {
        var bomb = AppData.FindProjectile<Bomb>(name);

        decimal bombDamage;
        var ricochetAngle = "";
        var armingThreshold = 0;
        decimal fuseTimer = 0;
        var showBlastPenetration = true;
        decimal fireChance = 0;

        if (bomb.BombType.Equals(WoWsShipBuilder.DataStructures.BombType.AP))
        {
            List<float> bombDamageModifiers = modifiers.FindModifiers("bombApAlphaDamageMultiplier").ToList();
            bombDamage = (decimal)bombDamageModifiers.Aggregate(bomb.Damage, (current, modifier) => current * modifier);
            ricochetAngle = $"{bomb.RicochetAngle}-{bomb.AlwaysRicochetAngle}";
            armingThreshold = (int)bomb.ArmingThreshold;
            fuseTimer = (decimal)bomb.FuseTimer;
            showBlastPenetration = false;
        }
        else
        {
            List<float> bombDamageModifiers = modifiers.FindModifiers("bombAlphaDamageMultiplier").ToList();
            bombDamage = (decimal)bombDamageModifiers.Aggregate(bomb.Damage, (current, modifier) => current * modifier);
            var fireChanceModifiers = modifiers.FindModifiers("bombBurnChanceBonus");
            fireChance = (decimal)fireChanceModifiers.Aggregate(bomb.FireChance, (current, modifier) => current + modifier);
            var fireChanceModifiersBombs = modifiers.FindModifiers("burnChanceFactorBig");
            fireChance = fireChanceModifiersBombs.Aggregate(fireChance, (current, modifier) => current + (decimal)modifier);
        }

        var bombDataContainer = new BombDataContainer
        {
            Name = bomb.Name,
            BombType = $"ArmamentType_{bomb.BombType.BombTypeToString()}",
            Damage = Math.Round(bombDamage, 2),
            Penetration = (int)Math.Truncate(bomb.Penetration),
            FuseTimer = fuseTimer,
            ArmingThreshold = armingThreshold,
            RicochetAngles = ricochetAngle,
            FireChance = Math.Round(fireChance * 100, 1),
            ExplosionRadius = (decimal)bomb.ExplosionRadius,
            SplashCoeff = (decimal)bomb.SplashCoeff,
            ShowBlastPenetration = showBlastPenetration,
            SplashRadius = Math.Round((decimal)bomb.DepthSplashRadius, 1),
            SplashDmg = Math.Round(bombDamage * (decimal)bomb.SplashDamageCoefficient),
        };

        bombDataContainer.UpdateDataElements();

        return bombDataContainer;
    }

    private bool ShouldDisplayBlastPenetration(object obj)
    {
        return ShowBlastPenetration;
    }
}
