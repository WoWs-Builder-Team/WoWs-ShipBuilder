using WoWsShipBuilder.DataStructures.Ship.Components;

namespace WoWsShipBuilder.Features.ShipStats;

public class TurretListComparer : IComparer<IGun>
{
#pragma warning disable CA1725
    public int Compare(IGun? firstGun, IGun? secondGun)
#pragma warning restore CA1725
    {
        if (firstGun == null || secondGun == null)
        {
            throw new InvalidOperationException("Cannot process null values");
        }

        if (firstGun.VerticalPosition < 3)
        {
            return firstGun.VerticalPosition.CompareTo(secondGun.VerticalPosition);
        }

        return secondGun.VerticalPosition.CompareTo(firstGun.VerticalPosition);
    }
}
