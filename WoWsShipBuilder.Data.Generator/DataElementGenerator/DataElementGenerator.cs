using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WoWsShipBuilder.Data.Generator.Utilities;

namespace WoWsShipBuilder.Data.Generator.DataElementGenerator;

[Generator(LanguageNames.CSharp)]
public class DataElementGenerator : IIncrementalGenerator
{
    private const string DataContainerAttributeFullName = $"{AttributeHelper.AttributeNamespace}.{AttributeHelper.DataContainerAttributeName}";
    private const string DataElementAttributeFullName = $"{AttributeHelper.AttributeNamespace}.{AttributeHelper.DataElementTypeAttributeName}";
    private const string DataElementNamespace = "global::WoWsShipBuilder.DataElements.DataElements";
    private const string DataElementsCollectionName = "this.DataElements";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(AttributeHelper.GenerateAttributes);
        var model = context.SyntaxProvider
            .ForAttributeWithMetadataName(DataContainerAttributeFullName, CouldBeDataContainer, GetModel)
            .Select(ExtractPropertyGroups);

        context.RegisterSourceOutput(model, GenerateSourceCode);
    }

    private static bool CouldBeDataContainer(SyntaxNode syntaxNode, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return syntaxNode is RecordDeclarationSyntax typeDeclarationSyntax && typeDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }

    private static ContainerData GetModel(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        var recordSymbol = (INamedTypeSymbol)context.TargetSymbol;
        token.ThrowIfCancellationRequested();
        var name = recordSymbol.Name;
        var dataNamespace = recordSymbol.ContainingNamespace.ToDisplayString();
        var properties = recordSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(prop => prop.HasAttributeWithFullName(DataElementAttributeFullName))
            .Select(RefineProperty)
            .ToEquatableArray();

        token.ThrowIfCancellationRequested();
        return new(name, dataNamespace, properties, null);
    }

    private static ContainerData ExtractPropertyGroups(ContainerData rawContainerData, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var propertyGroups = rawContainerData.Properties
            .Where(prop => (prop.DataElementType & DataElementTypes.Grouped) == DataElementTypes.Grouped)
            .GroupBy(prop => prop.DisplayOptions.GroupKey!)
            .Select(grouping => new PropertyGroup(grouping.Key, grouping.Select(prop => prop with { DataElementType = prop.DataElementType & ~DataElementTypes.Grouped }).ToEquatableArray()))
            .ToEquatableArray();
        token.ThrowIfCancellationRequested();
        var singleProperties = rawContainerData.Properties.Where(prop => prop.DisplayOptions.GroupKey is null).ToEquatableArray();

        token.ThrowIfCancellationRequested();
        return rawContainerData with { Properties = singleProperties, GroupedProperties = propertyGroups };
    }

    private sealed record ContainerData(string ContainerName, string Namespace, EquatableArray<PropertyData> Properties, EquatableArray<PropertyGroup>? GroupedProperties);

    private sealed record PropertyGroup(string GroupName, EquatableArray<PropertyData> Properties);

    private sealed record PropertyData(string Name, bool IsString, bool IsNullable, DataElementTypes DataElementType, PropertyDisplayOptions DisplayOptions, PropertyFilter PropertyFilter, FormattedTextData FormattedTextData);

    private sealed record PropertyDisplayOptions(string? UnitKey, string? LocalizationKey, string? TooltipKey, string? GroupKey, bool TreatValueAsLocalizationKey, bool TreatValueAsAppLocalizationKey);

    private sealed record FormattedTextData(string? ArgumentsCollectionName, bool TreatArgumentsAsLocalizationKeys, bool TreatArgumentsAsAppLocalizationKeys);

    private sealed record PropertyFilter(bool IsEnabled, string FilterMethodName);

    private static PropertyData RefineProperty(IPropertySymbol propertySymbol)
    {
        var dataElementAttribute = propertySymbol.FindAttribute(DataElementAttributeFullName);
        var dataElementType = (DataElementTypes)dataElementAttribute.ConstructorArguments[0].Value!;

        return new(propertySymbol.Name, propertySymbol.Type.SpecialType == SpecialType.System_String, propertySymbol.NullableAnnotation == NullableAnnotation.Annotated, dataElementType, ExtractDisplayOptions(propertySymbol, dataElementAttribute), ExtractFilterOptions(propertySymbol), ExtractFormattedTextOptions(dataElementAttribute));
    }

    private static FormattedTextData ExtractFormattedTextOptions(AttributeData dataElementAttribute)
    {
        return new(
            dataElementAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "ValuesPropertyName").Value.Value?.ToString(),
            (bool?)dataElementAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "ArePropertyNameValuesKeys").Value.Value ?? false,
            (bool?)dataElementAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "IsPropertyNameValuesAppLocalization").Value.Value ?? false);
    }

    private static PropertyFilter ExtractFilterOptions(IPropertySymbol propertySymbol)
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

    private static PropertyDisplayOptions ExtractDisplayOptions(ISymbol propertySymbol, AttributeData dataElementAttribute)
    {
        return new(
            dataElementAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "UnitKey").Value.Value?.ToString(),
            dataElementAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "NameLocalizationKey").Value.Value?.ToString() ?? propertySymbol.Name,
            dataElementAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "TooltipKey").Value.Value?.ToString(),
            dataElementAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "GroupKey").Value.Value?.ToString(),
            (bool?)dataElementAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "IsValueLocalizationKey").Value.Value ?? false,
            (bool?)dataElementAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "IsValueAppLocalization").Value.Value ?? false);
    }

    private static void GenerateSourceCode(SourceProductionContext context, ContainerData containerData)
    {
        var builder = new SourceBuilder();
        builder.Line("// <auto-generated />");
        builder.Line("#nullable enable");
        using (builder.Namespace(containerData.Namespace))
        {
            using (builder.Record(containerData.ContainerName))
            {
                using (builder.Block("private void UpdateDataElements()"))
                {
                    GenerateUpdateMethodContent(builder, containerData);
                }
            }
        }

        context.AddSource($"{containerData.ContainerName}.g.cs", builder.ToString());
    }

    private static void GenerateUpdateMethodContent(SourceBuilder builder, ContainerData containerData)
    {
        builder.Line("this.DataElements.Clear();");
        foreach (var propertyGroup in containerData.GroupedProperties ?? EquatableArray<PropertyGroup>.Empty)
        {
            var listName = $"{propertyGroup.GroupName}List";
            builder.Line($"var {listName} = new global::System.Collections.Generic.List<{DataElementNamespace}.IDataElement>();");
            foreach (var property in propertyGroup.Properties)
            {
                GenerateGroupedPropertyCode(builder, property, listName);
            }

            using (builder.Block($"if ({listName}.Count > 0)"))
            {
                builder.Line($"{DataElementsCollectionName}.Add(new {DataElementNamespace}.GroupedDataElement(\"ShipStats_{propertyGroup.GroupName}\", {listName}));");
            }
        }

        foreach (var property in containerData.Properties)
        {
            GeneratePropertyCode(builder, property);
        }
    }

    private static void GenerateGroupedPropertyCode(SourceBuilder builder, PropertyData property, string listName)
    {
        if (property.PropertyFilter.IsEnabled)
        {
            var filterMethodName = property.PropertyFilter.FilterMethodName;
            using (builder.Block($"if ({filterMethodName}(this.{property.Name}))"))
            {
                builder.Line($"{listName}.Add({GenerateDataElementCreationCode(property)});");
            }
        }
        else
        {
            builder.Line($"{listName}.Add({GenerateDataElementCreationCode(property)});");
        }
    }

    private static void GeneratePropertyCode(SourceBuilder builder, PropertyData property)
    {
        if (property.PropertyFilter.IsEnabled)
        {
            var filterMethodName = property.PropertyFilter.FilterMethodName;
            using (builder.Block($"if ({filterMethodName}(this.{property.Name}))"))
            {
                builder.Line($"{DataElementsCollectionName}.Add({GenerateDataElementCreationCode(property)});");
            }
        }
        else
        {
            builder.Line($"{DataElementsCollectionName}.Add({GenerateDataElementCreationCode(property)});");
        }
    }

    private static string GenerateDataElementCreationCode(PropertyData propertyData)
    {
        return propertyData.DataElementType switch
        {
            DataElementTypes.Value => $"new {DataElementNamespace}.ValueDataElement({GeneratePropertyAccess(propertyData)}, {propertyData.DisplayOptions.TreatValueAsLocalizationKey.ToLowerString()}, {propertyData.DisplayOptions.TreatValueAsAppLocalizationKey.ToLowerString()})",
            DataElementTypes.KeyValue => $"""new {DataElementNamespace}.KeyValueDataElement("ShipStats_{propertyData.DisplayOptions.LocalizationKey}", {GeneratePropertyAccess(propertyData)}, {propertyData.DisplayOptions.TreatValueAsLocalizationKey.ToLowerString()}, {propertyData.DisplayOptions.TreatValueAsAppLocalizationKey.ToLowerString()})""",
            DataElementTypes.KeyValueUnit => $"""new {DataElementNamespace}.KeyValueUnitDataElement("ShipStats_{propertyData.DisplayOptions.LocalizationKey}", {GeneratePropertyAccess(propertyData)}, "Unit_{propertyData.DisplayOptions.UnitKey}")""",
            DataElementTypes.FormattedText => $"new {DataElementNamespace}.FormattedTextDataElement({GeneratePropertyAccess(propertyData)}, this.{propertyData.FormattedTextData.ArgumentsCollectionName}, {propertyData.DisplayOptions.TreatValueAsLocalizationKey.ToLowerString()}, {propertyData.DisplayOptions.TreatValueAsAppLocalizationKey.ToLowerString()}, {propertyData.FormattedTextData.TreatArgumentsAsLocalizationKeys.ToLowerString()}, {propertyData.FormattedTextData.TreatArgumentsAsAppLocalizationKeys.ToLowerString()})",
            DataElementTypes.Tooltip => $"""new {DataElementNamespace}.TooltipDataElement("ShipStats_{propertyData.DisplayOptions.LocalizationKey}", {GeneratePropertyAccess(propertyData)}, "ShipStats_{propertyData.DisplayOptions.TooltipKey}", "{ComputeNullableUnitValue(propertyData.DisplayOptions)}")""",
            _ => throw new InvalidOperationException($"Invalid DataElementType: {propertyData.DataElementType}")
        };
    }

    private static string ComputeNullableUnitValue(PropertyDisplayOptions displayOptions) => displayOptions.UnitKey is not null ? $"Unit_{displayOptions.UnitKey}" : string.Empty;

    private static string GeneratePropertyAccess(PropertyData propertyData)
    {
        return propertyData switch
        {
            { IsString: true, IsNullable: false } => $"this.{propertyData.Name}",
            { IsString: true, IsNullable: true } => $"this.{propertyData.Name} ?? \"null\"",
            { IsString: false, IsNullable: false } => $"this.{propertyData.Name}.ToString()",
            { IsString: false, IsNullable: true } => $"this.{propertyData.Name}?.ToString() ?? \"null\"",
        };
    }
}
