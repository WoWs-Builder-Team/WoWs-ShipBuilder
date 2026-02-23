namespace WoWsShipBuilder.Infrastructure.Utility;

[AttributeUsage(AttributeTargets.Field)]
public class ObservableAttribute : Attribute
{
    public enum Visibility
    {
        Private, Protected, Internal, Public,
    }

    public Visibility SetterVisibility { get; set; } = Visibility.Public;

    public string[]? Dependants { get; set; }
}
