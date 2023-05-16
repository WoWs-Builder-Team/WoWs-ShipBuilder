﻿namespace WoWsShipBuilder.Infrastructure.DataTransfer;

public sealed record GunDataContainer(decimal HPos, decimal VPos, decimal BaseAngle, decimal[] Sector, decimal[][] DeadZones);
