namespace WoWsShipBuilder.Core.DataUI;

public interface IDataUi
{
    protected static bool ShouldAdd(object? value)
    {
        return value switch
        {
            string strValue => !string.IsNullOrEmpty(strValue),
            decimal decValue => decValue != 0,
            (decimal min, decimal max) => min > 0 || max > 0,
            int intValue => intValue != 0,
            _ => false,
        };
    }
}
