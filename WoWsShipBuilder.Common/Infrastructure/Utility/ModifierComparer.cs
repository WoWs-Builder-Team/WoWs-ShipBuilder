using WoWsShipBuilder.DataStructures.Modifiers;

namespace WoWsShipBuilder.Infrastructure.Utility;

public class ModifierComparer : IEqualityComparer<Modifier>
{
    public static ModifierComparer Instance { get; } = new();

    public bool Equals(Modifier? x, Modifier? y)
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

        return x.Name == y.Name && Math.Abs(x.Value - y.Value) < 0.0001;
    }

    public int GetHashCode(Modifier obj)
    {
        return HashCode.Combine(obj.Name, obj.Value);
    }
}
