using System.Collections.Immutable;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Projectile;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.GameData;

namespace WoWsShipBuilder.Features.DataContainers;

[DataContainer]
public partial class TorpedoDataContainer : ProjectileDataContainer
{
    [DataElementType(DataElementTypes.KeyValue, ValueTextKind = TextKind.AppLocalizationKey)]
    public string TorpedoType { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValue, ValueTextKind = TextKind.LocalizationKey)]
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

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxTurningSpeed", UnitKey = "DegreePerSecond", LocalizationKeyOverride = "FirstPing")]
    public decimal MaxTurningSpeedFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxTurningSpeed", UnitKey = "DegreePerSecond", LocalizationKeyOverride = "SecondPing")]
    public decimal MaxTurningSpeedSecondPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "TurningAcceleration", UnitKey = "DegreePerSecond2", LocalizationKeyOverride = "FirstPing")]
    public decimal TurningAccelerationFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "TurningAcceleration", UnitKey = "DegreePerSecond2", LocalizationKeyOverride = "SecondPing")]
    public decimal TurningAccelerationSecondPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxVerticalSpeed", UnitKey = "MPS", LocalizationKeyOverride = "FirstPing")]
    public decimal MaxVerticalSpeedFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxVerticalSpeed", UnitKey = "MPS", LocalizationKeyOverride = "SecondPing")]
    public decimal MaxVerticalSpeedSecondPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "VerticalAcceleration", UnitKey = "MPS2", LocalizationKeyOverride = "FirstPing")]
    public decimal VerticalAccelerationFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "VerticalAcceleration", UnitKey = "MPS2", LocalizationKeyOverride = "SecondPing")]
    public decimal VerticalAccelerationSecondPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "SearchRadius", UnitKey = "KM", LocalizationKeyOverride = "FirstPing")]
    public decimal SearchRadiusFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "SearchRadius", UnitKey = "KM", LocalizationKeyOverride = "SecondPing")]
    public decimal SearchRadiusSecondPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "SearchAngle", UnitKey = "Degree", LocalizationKeyOverride = "FirstPing")]
    public decimal SearchAngleFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "SearchAngle", UnitKey = "Degree", LocalizationKeyOverride = "SecondPing")]
    public decimal SearchAngleSecondPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Destroyer", UnitKey = "M", LocalizationKeyOverride = "FirstPing")]
    public decimal DestroyerCutOffFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Destroyer", UnitKey = "M", LocalizationKeyOverride = "SecondPing")]
    public decimal DestroyerCutOffSecondPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Battleship", UnitKey = "M", LocalizationKeyOverride = "FirstPing")]
    public decimal BattleshipCutOffFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Battleship", UnitKey = "M", LocalizationKeyOverride = "SecondPing")]
    public decimal BattleshipCutOffSecondPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Cruiser", UnitKey = "M", LocalizationKeyOverride = "FirstPing")]
    public decimal CruiserCutOffFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Cruiser", UnitKey = "M", LocalizationKeyOverride = "SecondPing")]
    public decimal CruiserCutOffSecondPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Submarine", UnitKey = "M", LocalizationKeyOverride = "FirstPing")]
    public decimal SubCutOffFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Submarine", UnitKey = "M", LocalizationKeyOverride = "SecondPing")]
    public decimal SubCutOffSecondPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "AirCarrier", UnitKey = "M", LocalizationKeyOverride = "FirstPing")]
    public decimal CvCutOffFirstPing { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "AirCarrier", UnitKey = "M", LocalizationKeyOverride = "SecondPing")]
    public decimal CvCutOffSecondPing { get; set; }

    public List<ShipClass>? CanHitClasses { get; set; }

    public bool IsLast { get; set; }

    public bool IsFromPlane { get; set; }

    public static List<TorpedoDataContainer> FromTorpedoName(IEnumerable<string> torpedoNames, ImmutableList<Modifier> modifiers, bool fromPlane)
    {
        var list = new List<TorpedoDataContainer>();
        foreach (string name in torpedoNames)
        {
            var torp = AppData.FindProjectile<Torpedo>(name);

            var torpedoDamage = modifiers.ApplyModifiers("TorpedoDataContainer.Damage", (decimal)torp.Damage);

            var torpedoSpeed = modifiers.ApplyModifiers(fromPlane ? "TorpedoDataContainer.Damage.Plane" : "TorpedoDataContainer.Damage.Ship", (decimal)torp.Speed);

            var torpedoDetect = modifiers.ApplyModifiers("TorpedoDataContainer.Visibility", (decimal)torp.SpottingRange);

            var torpedoArmingTime = modifiers.ApplyModifiers("TorpedoDataContainer.ArmingTime", (decimal)torp.ArmingTime);

            var torpedoFlooding = modifiers.ApplyModifiers(fromPlane ? "TorpedoDataContainer.FloodChance.Plane" : "TorpedoDataContainer.FloodChance.Ship", (decimal)torp.FloodChance);

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
                torpedoDataContainer.MaxTurningSpeedFirstPing = Math.Round((decimal)torp.MagneticTorpedoParams!.MaxTurningSpeed[0], 1);
                torpedoDataContainer.MaxTurningSpeedSecondPing = Math.Round((decimal)torp.MagneticTorpedoParams.MaxTurningSpeed[^1], 1);
                torpedoDataContainer.TurningAccelerationFirstPing = Math.Round((decimal)torp.MagneticTorpedoParams.TurningAcceleration[0], 1);
                torpedoDataContainer.TurningAccelerationSecondPing = Math.Round((decimal)torp.MagneticTorpedoParams.TurningAcceleration[^1], 1);
                torpedoDataContainer.MaxVerticalSpeedFirstPing = Math.Round((decimal)torp.MagneticTorpedoParams.MaxVerticalSpeed[0], 1);
                torpedoDataContainer.MaxVerticalSpeedSecondPing = Math.Round((decimal)torp.MagneticTorpedoParams.MaxVerticalSpeed[^1], 1);
                torpedoDataContainer.VerticalAccelerationFirstPing = Math.Round((decimal)torp.MagneticTorpedoParams.VerticalAcceleration[0], 1);
                torpedoDataContainer.VerticalAccelerationSecondPing = Math.Round((decimal)torp.MagneticTorpedoParams.VerticalAcceleration[^1], 1);
                torpedoDataContainer.SearchRadiusFirstPing = Math.Round((decimal)torp.MagneticTorpedoParams.SearchRadius[0], 1);
                torpedoDataContainer.SearchRadiusSecondPing = Math.Round((decimal)torp.MagneticTorpedoParams.SearchRadius[^1], 1);
                torpedoDataContainer.SearchAngleFirstPing = Math.Round((decimal)torp.MagneticTorpedoParams.SearchAngle[0], 1);
                torpedoDataContainer.SearchAngleSecondPing = Math.Round((decimal)torp.MagneticTorpedoParams.SearchAngle[^1], 1);

                var ddCutOff = torp.MagneticTorpedoParams.DropTargetAtDistance.First(x => x.Key == ShipClass.Destroyer).Value;
                var bbCutOff = torp.MagneticTorpedoParams.DropTargetAtDistance.First(x => x.Key == ShipClass.Battleship).Value;
                var caCutOff = torp.MagneticTorpedoParams.DropTargetAtDistance.First(x => x.Key == ShipClass.Cruiser).Value;
                var subCutOff = torp.MagneticTorpedoParams.DropTargetAtDistance.First(x => x.Key == ShipClass.Submarine).Value;
                var cvCutOff = torp.MagneticTorpedoParams.DropTargetAtDistance.First(x => x.Key == ShipClass.AirCarrier).Value;

                torpedoDataContainer.DestroyerCutOffFirstPing = Math.Round((decimal)ddCutOff[0], 1);
                torpedoDataContainer.DestroyerCutOffSecondPing = Math.Round((decimal)ddCutOff[^1], 1);
                torpedoDataContainer.BattleshipCutOffFirstPing = Math.Round((decimal)bbCutOff[0], 1);
                torpedoDataContainer.BattleshipCutOffSecondPing = Math.Round((decimal)bbCutOff[^1], 1);
                torpedoDataContainer.CruiserCutOffFirstPing = Math.Round((decimal)caCutOff[0], 1);
                torpedoDataContainer.CruiserCutOffSecondPing = Math.Round((decimal)caCutOff[^1], 1);
                torpedoDataContainer.SubCutOffFirstPing = Math.Round((decimal)subCutOff[0], 1);
                torpedoDataContainer.SubCutOffSecondPing = Math.Round((decimal)subCutOff[^1], 1);
                torpedoDataContainer.CvCutOffFirstPing = Math.Round((decimal)cvCutOff[0], 1);
                torpedoDataContainer.CvCutOffSecondPing = Math.Round((decimal)cvCutOff[^1], 1);
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
        return this.IsFromPlane;
    }
}
