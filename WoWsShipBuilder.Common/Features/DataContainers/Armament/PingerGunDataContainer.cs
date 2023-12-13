using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.DataContainers;

[DataContainer]
public partial record PingerGunDataContainer : DataContainerBase
{
    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal Reload { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "DegreePerSecond")]
    public decimal TraverseSpeed { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal TurnTime { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "KM")]
    public decimal Range { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "PingDuration", UnitKey = "S", LocalizationKeyOverride = "First")]
    public decimal FirstPingDuration { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "PingDuration", UnitKey = "S", LocalizationKeyOverride = "Second")]
    public decimal SecondPingDuration { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "M")]
    public decimal PingWidth { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "MPS")]
    public decimal PingSpeed { get; set; }

    public static PingerGunDataContainer? FromShip(Ship ship, IEnumerable<ShipUpgrade> shipConfiguration, List<Modifier> modifiers)
    {
        if (!ship.PingerGunList.Any())
        {
            return null;
        }

        PingerGun pingerGun;
        var pingerUpgrade = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.Sonar);
        if (pingerUpgrade is null && ship.PingerGunList.Count is 1)
        {
            Logging.Logger.LogWarning("No sonar upgrade information found for ship {ShipName} even though there is one sonar module available", ship.Name);
            return null;
        }

        if (pingerUpgrade is null)
        {
            throw new InvalidOperationException($"No sonar upgrade information found for ship {ship.Name} but there is more than one sonar module.");
        }

        // Safe approach is necessary because data up until 0.11.9#1 does not include this data due to an issue in the data converter
        if (pingerUpgrade.Components.TryGetValue(ComponentType.Sonar, out ImmutableArray<string> pingerGunInfo))
        {
            pingerGun = ship.PingerGunList[pingerGunInfo[0]];
        }
        else
        {
            Logging.Logger.LogWarning("Unable to retrieve sonar component from upgrade info for ship {} and ship upgrade {}", ship.Index, pingerUpgrade.Name);
            pingerGun = ship.PingerGunList.First().Value;
        }

        var pingSpeed = pingerGun.WaveParams[0].WaveSpeed[0];
        pingSpeed = modifiers.ApplyModifiers("PingerGunDataContainer.WaveSpeed", pingSpeed);

        var firstPingDuration = pingerGun.SectorParams[0].Lifetime;
        firstPingDuration = modifiers.ApplyModifiers("PingerGunDataContainer.PingDuration.First", firstPingDuration);

        var secondPingDuration = pingerGun.SectorParams[1].Lifetime;
        secondPingDuration = modifiers.ApplyModifiers("PingerGunDataContainer.PingDuration.Second", secondPingDuration);

        var traverseSpeed = pingerGun.RotationSpeed[0];

        var reload = modifiers.ApplyModifiers("PingerGunDataContainer.Reload", pingerGun.WaveReloadTime);

        var pingerGunDataContainer = new PingerGunDataContainer
        {
            TurnTime = Math.Round(180 / traverseSpeed, 1),
            TraverseSpeed = traverseSpeed,
            Reload = Math.Round(reload, 2),
            Range = pingerGun.WaveDistance / 1000,
            FirstPingDuration = Math.Round(firstPingDuration, 1),
            SecondPingDuration = Math.Round(secondPingDuration, 1),
            PingWidth = pingerGun.WaveParams[0].StartWaveWidth,
            PingSpeed = Math.Round(pingSpeed, 0),
        };

        pingerGunDataContainer.UpdateDataElements();

        return pingerGunDataContainer;
    }
}
