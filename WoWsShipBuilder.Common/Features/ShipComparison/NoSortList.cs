namespace WoWsShipBuilder.Features.ShipComparison;

public class NoSortList<T> : List<T>, IComparable
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
