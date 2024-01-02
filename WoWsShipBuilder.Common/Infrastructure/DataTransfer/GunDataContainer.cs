using System.Collections.Immutable;

namespace WoWsShipBuilder.Infrastructure.DataTransfer;

public sealed record GunDataContainer(decimal HPos, decimal VPos, decimal BaseAngle, ImmutableArray<decimal> Sector, ImmutableArray<ImmutableArray<decimal>> DeadZones);
