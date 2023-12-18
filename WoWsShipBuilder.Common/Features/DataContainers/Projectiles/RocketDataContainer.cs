using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Projectile;
using WoWsShipBuilder.Features.BallisticCharts;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.GameData;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.DataContainers;

[DataContainer]
public partial record RocketDataContainer : ProjectileDataContainer
{
    [DataElementType(DataElementTypes.KeyValue, ValueTextKind = TextKind.AppLocalizationKey)]
    public string RocketType { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValue, ValueTextKind = TextKind.LocalizationKey)]
    public string Name { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValue)]
    public decimal Damage { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = "Splash", TooltipKey = "SplashExplanation", UnitKey = "M")]
    public decimal SplashRadius { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = "Splash", TooltipKey = "SplashExplanation")]
    public decimal SplashDmg { get; set; }

    [DataElementType(DataElementTypes.Tooltip, TooltipKey = "KruppExplanation")]
    public decimal Krupp { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "MM")]
    public int Penetration { get; set; }

    [DataElementType(DataElementTypes.Tooltip, TooltipKey = "ApPenetrationFormula", UnitKey = "MM")]
    public int PenetrationAp { get; set; }

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

    public static RocketDataContainer FromRocketName(string name, List<Modifier> modifiers)
    {
        var rocket = AppData.FindProjectile<Rocket>(name);

        var rocketDamage = (decimal)rocket.Damage;
        var showBlastPenetration = true;
        var ricochetAngle = "";
        decimal fuseTimer = 0;
        var armingThreshold = 0;
        decimal fireChance = 0;
        int penetrationHe = 0;
        int penetrationAp = 0;
        if (rocket.RocketType.Equals(DataStructures.RocketType.AP))
        {
            rocketDamage = modifiers.ApplyModifiers("RocketDataContainer.Damage.Ap", rocketDamage);
            ricochetAngle = $"{rocket.RicochetAngle}-{rocket.AlwaysRicochetAngle}";
            fuseTimer = (decimal)rocket.FuseTimer;
            armingThreshold = (int)rocket.ArmingThreshold;
            showBlastPenetration = false;
            penetrationAp = (int)Math.Round(BallisticHelper.CalculatePen(rocket.MuzzleVelocity, rocket.Caliber, rocket.Mass, rocket.Krupp));
        }
        else
        {
            fireChance = modifiers.ApplyModifiers("RocketDataContainer.FireChance", (decimal)rocket.FireChance);
            penetrationHe = (int)Math.Truncate(rocket.Penetration);
        }

        var rocketDataContainer = new RocketDataContainer
        {
            Name = rocket.Name,
            RocketType = $"ArmamentType_{rocket.RocketType.RocketTypeToString()}",
            Damage = Math.Round(rocketDamage, 2),
            Penetration = penetrationHe,
            PenetrationAp = penetrationAp,
            FuseTimer = fuseTimer,
            ArmingThreshold = armingThreshold,
            RicochetAngles = ricochetAngle,
            FireChance = Math.Round(fireChance * 100, 1),
            ExplosionRadius = (decimal)rocket.ExplosionRadius,
            SplashCoeff = (decimal)rocket.SplashCoeff,
            ShowBlastPenetration = showBlastPenetration,
            SplashRadius = Math.Round((decimal)rocket.DepthSplashRadius, 1),
            SplashDmg = Math.Round(rocketDamage * (decimal)rocket.SplashDamageCoefficient),
            Krupp = (decimal)rocket.Krupp,
        };

        rocketDataContainer.UpdateDataElements();

        return rocketDataContainer;
    }

    private bool ShouldDisplayBlastPenetration(object obj)
    {
        return this.ShowBlastPenetration;
    }
}
