using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Infrastructure.Utility;

public class ShipUpgradeComparer : IEqualityComparer<ShipUpgrade>
{
    public static ShipUpgradeComparer Instance { get; } = new();

    public bool Equals(ShipUpgrade? x, ShipUpgrade? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null))
        {
            return false;
        }

        if (ReferenceEquals(y, null))
        {
            return false;
        }

        return x.Name == y.Name;
    }

    public int GetHashCode(ShipUpgrade obj) => obj.Name.GetHashCode();
}
