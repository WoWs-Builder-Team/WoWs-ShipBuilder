using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Projectile;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Infrastructure.Data;

namespace WoWsShipBuilder.DataContainers;

public partial record TorpedoDataContainer : ProjectileDataContainer
{
    [DataElementType(DataElementTypes.KeyValue, IsValueLocalizationKey = true, IsValueAppLocalization = true)]
    public string TorpedoType { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValue, IsValueLocalizationKey = true)]
    [DataElementFiltering(true, "ShouldDisplayName")]
    public string Name { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValue)]
    public decimal Damage { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "KM")]
    public decimal Range { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Knots")]
    public decimal Speed { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "KM")]
    public decimal Detectability { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "M")]
    public int ArmingDistance { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal ReactionTime { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "PerCent")]
    public decimal FloodingChance { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Blast", UnitKey = "M")]
    public decimal ExplosionRadius { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = "Blast", TooltipKey = "BlastExplanation")]
    [DataElementFiltering(false)]
    public decimal SplashCoeff { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxTurningSpeed", UnitKey = "DegreePerSecond", NameLocalizationKey = "FirstPing")]
    public decimal MaxTurningSpeedFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxTurningSpeed", UnitKey = "DegreePerSecond", NameLocalizationKey = "SecondPing")]
    public decimal MaxTurningSpeedSecondPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "TurningAcceleration", UnitKey = "DegreePerSecond2", NameLocalizationKey = "FirstPing")]
    public decimal TurningAccelerationFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "TurningAcceleration", UnitKey = "DegreePerSecond2", NameLocalizationKey = "SecondPing")]
    public decimal TurningAccelerationSecondPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxVerticalSpeed", UnitKey = "MPS", NameLocalizationKey = "FirstPing")]
    public decimal MaxVerticalSpeedFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxVerticalSpeed", UnitKey = "MPS", NameLocalizationKey = "SecondPing")]
    public decimal MaxVerticalSpeedSecondPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "VerticalAcceleration", UnitKey = "MPS2", NameLocalizationKey = "FirstPing")]
    public decimal VerticalAccelerationFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "VerticalAcceleration", UnitKey = "MPS2", NameLocalizationKey = "SecondPing")]
    public decimal VerticalAccelerationSecondPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "SearchRadius", UnitKey = "KM", NameLocalizationKey = "FirstPing")]
    public decimal SearchRadiusFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "SearchRadius", UnitKey = "KM", NameLocalizationKey = "SecondPing")]
    public decimal SearchRadiusSecondPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "SearchAngle", UnitKey = "Degree", NameLocalizationKey = "FirstPing")]
    public decimal SearchAngleFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "SearchAngle", UnitKey = "Degree", NameLocalizationKey = "SecondPing")]
    public decimal SearchAngleSecondPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Destroyer", UnitKey = "M", NameLocalizationKey = "FirstPing")]
    public decimal DestroyerCutOffFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Destroyer", UnitKey = "M", NameLocalizationKey = "SecondPing")]
    public decimal DestroyerCutOffSecondPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Battleship", UnitKey = "M", NameLocalizationKey = "FirstPing")]
    public decimal BattleshipCutOffFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Battleship", UnitKey = "M", NameLocalizationKey = "SecondPing")]
    public decimal BattleshipCutOffSecondPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Cruiser", UnitKey = "M", NameLocalizationKey = "FirstPing")]
    public decimal CruiserCutOffFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Cruiser", UnitKey = "M", NameLocalizationKey = "SecondPing")]
    public decimal CruiserCutOffSecondPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Submarine", UnitKey = "M", NameLocalizationKey = "FirstPing")]
    public decimal SubCutOffFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Submarine", UnitKey = "M", NameLocalizationKey = "SecondPing")]
    public decimal SubCutOffSecondPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "AirCarrier", UnitKey = "M", NameLocalizationKey = "FirstPing")]
    public decimal CvCutOffFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "AirCarrier", UnitKey = "M", NameLocalizationKey = "SecondPing")]
    public decimal CvCutOffSecondPing { get; set; }

    public List<ShipClass>? CanHitClasses { get; set; }

    public bool IsLast { get; set; }

    public bool IsFromPlane { get; set; }

    public static List<TorpedoDataContainer> FromTorpedoName(List<string> torpedoNames, List<(string name, float value)> modifiers, bool fromPlane)
    {
        var list = new List<TorpedoDataContainer>();
        foreach (string name in torpedoNames)
        {
            var torp = AppData.FindProjectile<Torpedo>(name);

            var torpedoDamageModifiers = modifiers.FindModifiers("torpedoDamageCoeff");
            var torpedoDamage = (decimal)torpedoDamageModifiers.Aggregate(torp.Damage, (current, modifier) => current * modifier);

            var torpedoSpeedModifiers = fromPlane ? modifiers.FindModifiers("planeTorpedoSpeedMultiplier") : modifiers.FindModifiers("torpedoSpeedMultiplier", true);
            var torpedoSpeed = (decimal)torpedoSpeedModifiers.Aggregate(torp.Speed, (current, modifier) => current * modifier);

            var torpedoDetectModifiers = modifiers.FindModifiers("torpedoVisibilityFactor");
            var torpedoDetect = (decimal)torpedoDetectModifiers.Aggregate(torp.SpottingRange, (current, modifier) => current * modifier);

            var torpedoArmingTimeModifiers = modifiers.FindModifiers("planeTorpedoArmingTimeCoeff");
            var torpedoArmingTime = (decimal)torpedoArmingTimeModifiers.Aggregate(torp.ArmingTime, (current, modifier) => current * modifier);

            var torpedoFloodingModifiers = fromPlane ? modifiers.FindModifiers("floodChanceFactorPlane", true) : modifiers.FindModifiers("floodChanceFactor", true);
            var torpedoFlooding = (decimal)torpedoFloodingModifiers.Aggregate(torp.FloodChance, (current, modifier) => current * modifier);

            var allClasses = new List<ShipClass> { ShipClass.Destroyer, ShipClass.Cruiser, ShipClass.Battleship, ShipClass.AirCarrier };

            // v = d/t --> d = v*t
            var torpedoDataContainer = new TorpedoDataContainer
            {
                Name = name,
                TorpedoType = $"ShipStats_Torpedo{torp.TorpedoType.TorpedoTypeToString()}",
                Damage = Math.Round(torpedoDamage),
                Range = Math.Round((decimal)torp.MaxRange / 1000, 1),
                Speed = Math.Round(torpedoSpeed, 2),
                Detectability = Math.Round(torpedoDetect, 2),
                ArmingDistance = (int)Math.Round(torpedoSpeed * Constants.KnotsToMps * torpedoArmingTime, 0),
                FloodingChance = Math.Round(torpedoFlooding * 100, 2),
                ReactionTime = Math.Round(torpedoDetect / (torpedoSpeed * Constants.KnotsToMps) * 1000, 2),
                ExplosionRadius = (decimal)torp.ExplosionRadius,
                SplashCoeff = (decimal)torp.SplashCoeff,
                IsFromPlane = fromPlane,
            };

            if (torp.TorpedoType == DataStructures.TorpedoType.Magnetic)
            {
                torpedoDataContainer.MaxTurningSpeedFirstPing = Math.Round((decimal)torp.MagneticTorpedoParams.MaxTurningSpeed.First(), 1);
                torpedoDataContainer.MaxTurningSpeedSecondPing = Math.Round((decimal)torp.MagneticTorpedoParams.MaxTurningSpeed.Last(), 1);
                torpedoDataContainer.TurningAccelerationFirstPing = Math.Round((decimal)torp.MagneticTorpedoParams.TurningAcceleration.First(), 1);
                torpedoDataContainer.TurningAccelerationSecondPing = Math.Round((decimal)torp.MagneticTorpedoParams.TurningAcceleration.Last(), 1);
                torpedoDataContainer.MaxVerticalSpeedFirstPing = Math.Round((decimal)torp.MagneticTorpedoParams.MaxVerticalSpeed.First(), 1);
                torpedoDataContainer.MaxVerticalSpeedSecondPing = Math.Round((decimal)torp.MagneticTorpedoParams.MaxVerticalSpeed.Last(), 1);
                torpedoDataContainer.VerticalAccelerationFirstPing = Math.Round((decimal)torp.MagneticTorpedoParams.VerticalAcceleration.First(), 1);
                torpedoDataContainer.VerticalAccelerationSecondPing = Math.Round((decimal)torp.MagneticTorpedoParams.VerticalAcceleration.Last(), 1);
                torpedoDataContainer.SearchRadiusFirstPing = Math.Round((decimal)torp.MagneticTorpedoParams.SearchRadius.First(), 1);
                torpedoDataContainer.SearchRadiusSecondPing = Math.Round((decimal)torp.MagneticTorpedoParams.SearchRadius.Last(), 1);
                torpedoDataContainer.SearchAngleFirstPing = Math.Round((decimal)torp.MagneticTorpedoParams.SearchAngle.First(), 1);
                torpedoDataContainer.SearchAngleSecondPing = Math.Round((decimal)torp.MagneticTorpedoParams.SearchAngle.Last(), 1);

                var ddCutOff = torp.MagneticTorpedoParams.DropTargetAtDistance.First(x => x.Key == ShipClass.Destroyer).Value;
                var bbCutOff = torp.MagneticTorpedoParams.DropTargetAtDistance.First(x => x.Key == ShipClass.Battleship).Value;
                var caCutOff = torp.MagneticTorpedoParams.DropTargetAtDistance.First(x => x.Key == ShipClass.Cruiser).Value;
                var subCutOff = torp.MagneticTorpedoParams.DropTargetAtDistance.First(x => x.Key == ShipClass.Submarine).Value;
                var cvCutOff = torp.MagneticTorpedoParams.DropTargetAtDistance.First(x => x.Key == ShipClass.AirCarrier).Value;

                torpedoDataContainer.DestroyerCutOffFirstPing = Math.Round((decimal)ddCutOff.First(), 1);
                torpedoDataContainer.DestroyerCutOffSecondPing = Math.Round((decimal)ddCutOff.Last(), 1);
                torpedoDataContainer.BattleshipCutOffFirstPing = Math.Round((decimal)bbCutOff.First(), 1);
                torpedoDataContainer.BattleshipCutOffSecondPing = Math.Round((decimal)bbCutOff.Last(), 1);
                torpedoDataContainer.CruiserCutOffFirstPing = Math.Round((decimal)caCutOff.First(), 1);
                torpedoDataContainer.CruiserCutOffSecondPing = Math.Round((decimal)caCutOff.Last(), 1);
                torpedoDataContainer.SubCutOffFirstPing = Math.Round((decimal)subCutOff.First(), 1);
                torpedoDataContainer.SubCutOffSecondPing = Math.Round((decimal)subCutOff.Last(), 1);
                torpedoDataContainer.CvCutOffFirstPing = Math.Round((decimal)cvCutOff.First(), 1);
                torpedoDataContainer.CvCutOffSecondPing = Math.Round((decimal)cvCutOff.Last(), 1);
            }

            if (torp.IgnoreClasses != null && torp.IgnoreClasses.Any())
            {
                torpedoDataContainer.CanHitClasses = allClasses.Except(torp.IgnoreClasses).ToList();
            }

            torpedoDataContainer.UpdateDataElements();
            list.Add(torpedoDataContainer);
        }

        return list;
    }

    private bool ShouldDisplayName(object obj)
    {
        return IsFromPlane;
    }
}
