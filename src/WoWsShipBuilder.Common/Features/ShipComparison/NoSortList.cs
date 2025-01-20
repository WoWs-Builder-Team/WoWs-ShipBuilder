namespace WoWsShipBuilder.Features.ShipComparison;

#pragma warning disable CA1036
public class NoSortList<T> : List<T>, IComparable
#pragma warning restore CA1036
{
    public NoSortList()
    {
    }

    public NoSortList(IEnumerable<T> collection)
        : base(collection)
    {
    }

    public int CompareTo(object? obj)
    {
        return 0;
    }
}
