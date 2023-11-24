using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.DataContainers;

public static class DataContainerUtility
{
    public static ShipDataContainer GetShipDataContainerFromBuild(Ship ship, IEnumerable<string> selectedModules, List<ShipUpgrade> shipConfiguration, List<Modifier> modifiers)
    {
        return ShipDataContainer.CreateFromShip(ship, Helpers.GetShipConfigurationFromBuild(selectedModules, shipConfiguration), modifiers);
    }

    public static ShipDataContainer GetStockShipDataContainer(Ship ship)
    {
        return ShipDataContainer.CreateFromShip(ship, Helpers.GetStockShipConfiguration(ship), new());
    }

    public static decimal ApplyModifiers(this List<Modifier> modifierList, string propertySelector, decimal initialValue)
    {
        return modifierList.FindAll(x => x.AffectedProperties.Contains(propertySelector)).Aggregate(initialValue, (total, current) => current.ApplyModifier(total));
    }

    public static int ApplyModifiers(this List<Modifier> modifierList, string propertySelector, int initialValue)
    {
        return modifierList.FindAll(x => x.AffectedProperties.Contains(propertySelector)).Aggregate(initialValue, (total, current) => current.ApplyModifier(total));
    }

    public static void UpdateConsumableModifierValue(this List<Modifier> consumableModifierList, List<Modifier> modifierList, string propertySelector, string modifierName)
    {
        var modifier = consumableModifierList.Find(x => x.Name.Equals(modifierName))!;
        var newValue = (float)modifierList.ApplyModifiers(propertySelector, (decimal)modifier.Value);
        consumableModifierList.Remove(modifier);
        consumableModifierList.Add(new Modifier(modifier.Name, newValue, "", modifier));
    }
}
