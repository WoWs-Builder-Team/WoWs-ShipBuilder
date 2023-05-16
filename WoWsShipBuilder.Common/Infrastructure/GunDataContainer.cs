namespace WoWsShipBuilder.Common.Infrastructure;

public sealed record GunDataContainer(decimal HPos, decimal VPos, decimal BaseAngle, decimal[] Sector, decimal[][] DeadZones);
