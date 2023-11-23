namespace WoWsShipBuilder.Features.ShipComparison;

public static class NoSortListExtensions
{
    public static NoSortList<TSource> ToNoSortList<TSource>(this IEnumerable<TSource> source)
    {
        return new(source);
    }
}
