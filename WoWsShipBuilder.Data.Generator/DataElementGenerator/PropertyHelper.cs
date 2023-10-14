using System.Linq;
using Microsoft.CodeAnalysis;
using WoWsShipBuilder.Data.Generator.DataElementGenerator.Model;
using WoWsShipBuilder.Data.Generator.Utilities;

namespace WoWsShipBuilder.Data.Generator.DataElementGenerator;

internal static class PropertyHelper
{
    private const string DataElementNamespace = "global::WoWsShipBuilder.DataElements";

    public static FormattedTextData ExtractFormattedTextOptions(AttributeData dataElementAttribute)
    {
        return new(
            dataElementAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "ArgumentsCollectionName").Value.Value?.ToString(),
            (TextKind)(dataElementAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "ArgumentsTextKind").Value.Value ?? TextKind.Plain));
    }

    public static PropertyDisplayOptions ExtractDisplayOptions(ISymbol propertySymbol, AttributeData dataElementAttribute)
    {
        return new(
            dataElementAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "UnitKey").Value.Value?.ToString(),
            dataElementAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "LocalizationKeyOverride").Value.Value?.ToString() ?? propertySymbol.Name,
            dataElementAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "TooltipKey").Value.Value?.ToString(),
            dataElementAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "GroupKey").Value.Value?.ToString(),
            (TextKind)(dataElementAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "ValueTextKind").Value.Value ?? TextKind.Plain));
    }

    public static PropertyFilter ExtractFilterOptions(IPropertySymbol propertySymbol)
    {
        var filterAttribute = propertySymbol.FindAttributeOrDefault("WoWsShipBuilder.DataElements.DataElementAttributes.DataElementFilteringAttribute");
        if (filterAttribute is null)
        {
            return new(true, $"{DataElementNamespace}.DataContainerBase.ShouldAdd");
        }

        var isEnabled = (bool)filterAttribute.ConstructorArguments[0].Value!;
        var filterMethodName = filterAttribute.ConstructorArguments[1].Value?.ToString() ?? $"{DataElementNamespace}.DataContainerBase.ShouldAdd";
        return new(isEnabled, filterMethodName);
    }
}
