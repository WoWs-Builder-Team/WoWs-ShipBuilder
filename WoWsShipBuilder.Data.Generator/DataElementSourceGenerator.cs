using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using WoWsShipBuilder.Data.Generator.DataElementGenerator;
using WoWsShipBuilder.Data.Generator.Utilities;

namespace WoWsShipBuilder.Data.Generator;

[Generator]
public class DataElementSourceGenerator : IIncrementalGenerator
{
    private const string DataContainerBaseName = "DataContainerBase";
    private const string ProjectileDataContainer = "ProjectileDataContainer";
    private const string GeneratedMethodName = "UpdateDataElements";
    private const string DataElementCollectionName = "DataElements";
    private const string Indentation = "        ";
    private const string IfIndentation = "    ";

    private static readonly DiagnosticDescriptor MissingAttributeError = new(id: "SB001",
        title: "A required secondary attribute is missing",
        messageFormat: "Couldn't find the required attribute property {0} for the DataElement type {1}",
        category: "Generator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidTypeEnumError = new(id: "SB002",
        title: "The enum type is invalid",
        messageFormat: "The enum type for the property {0} doesn't exist",
        category: "Generator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor GenerationError = new(id: "SB003",
        title: "Error during source generation",
        messageFormat: "There was an exception during source generation: {0}",
        category: "Generator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor TooManyIterationsError = new(id: "SB004",
        title: "Too many iterations",
        messageFormat: "Too many iteration for the grouped data element",
        category: "Generator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor GroupedMissingDefinitionError = new(id: "SB004",
        title: "Grouped require secondary type definition",
        messageFormat: "The property {0} is missing its own type definition. Add a type definition with the use of |.",
        category: "Generator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var dataClasses = context.SyntaxProvider.CreateSyntaxProvider(IsDataContainerRecord, GetRecordTypeOrNull)
            .Where(type => type is not null)
            .Select(GetPropertiesWithAttributes)
            .Where(x => x.properties.Count > 0)
            .Collect();

        context.RegisterSourceOutput(dataClasses, GenerateCode);
    }

    private static bool IsDataContainerRecord(SyntaxNode syntaxNode, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (syntaxNode is not RecordDeclarationSyntax recordSyntax)
        {
            return false;
        }

        bool baseName = recordSyntax.BaseList?.Types.ToString().Contains(DataContainerBaseName) ?? false;
        bool projectileDataContainerName = recordSyntax.BaseList?.Types.ToString().Contains(ProjectileDataContainer) ?? false;
        return recordSyntax.Modifiers.ToString().Contains("partial") && (baseName || projectileDataContainerName);
    }

    private static ITypeSymbol? GetRecordTypeOrNull(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var recordSyntax = (RecordDeclarationSyntax)context.Node;
        return context.SemanticModel.GetDeclaredSymbol(recordSyntax);
    }

    private static (string className, string classNamespace, List<IPropertySymbol> properties) GetPropertiesWithAttributes(ITypeSymbol? symbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var name = symbol!.Name;
        var dataNamespace = symbol.ContainingNamespace.ToDisplayString();
        if (symbol.HasAttributeWithFullName($"{AttributeHelper.AttributeNamespace}.{AttributeHelper.DataContainerAttributeName}"))
        {
            return (name, dataNamespace, new());
        }

        var properties = symbol.GetMembers().OfType<IPropertySymbol>().Where(prop => prop.GetAttributes().Any(attr => attr.AttributeClass!.Name == "DataElementTypeAttribute")).ToList();
        return (name, dataNamespace, properties);
    }

    private static void GenerateCode(SourceProductionContext context, ImmutableArray<(string className, string classNamespace, List<IPropertySymbol> properties)> dataRecords)
    {
        // context.AddSource("test.g.cs", SourceText.From("//" + string.Join(" - ",dataRecords.Select(x => x.Name)), Encoding.UTF8));
        foreach (var dataRecord in dataRecords)
        {
            var properties = dataRecord.properties;
            var classStart = $@"
using System;
using System.Collections.Generic;
using WoWsShipBuilder.DataElements.DataElements;

namespace {dataRecord.classNamespace};

#nullable enable
public partial record {dataRecord.className}
{{
    private void {GeneratedMethodName}()
    {{
        {DataElementCollectionName}.Clear();
";
            const string classEnd = @"
    }
}
#nullable restore
";
            var builder = new StringBuilder(classStart);

            while (properties.Any())
            {
                var prop = properties.First();
                var propertyAttributes = prop.GetAttributes();

                var typeAttribute = propertyAttributes.FirstOrDefault();
                if (typeAttribute is not null)
                {
                    if (typeAttribute.ConstructorArguments.IsEmpty)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(InvalidTypeEnumError, typeAttribute.ApplicationSyntaxReference!.GetSyntax().GetLocation(), prop.Name));
                        properties.RemoveAt(0);
                        continue;
                    }

                    try
                    {
                        var (code, additionalIndexes) = GenerateCode(context, typeAttribute, prop, propertyAttributes, properties, DataElementCollectionName, 0);
                        builder.Append(code);
                        builder.AppendLine();
                        foreach (var index in additionalIndexes.OrderByDescending(x => x))
                        {
                            properties.RemoveAt(index);
                        }
                    }
                    catch (Exception e)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(GenerationError, typeAttribute.ApplicationSyntaxReference!.GetSyntax().GetLocation(), e.Message + " - " + e.TargetSite));
                        break;
                    }
                }
            }

            builder.Append(classEnd);
            context.AddSource($"{dataRecord.className}.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
        }
    }

    private static (string code, List<int> additionalIndexes) GenerateCode(SourceProductionContext context, AttributeData typeAttribute, IPropertySymbol currentProp, ImmutableArray<AttributeData> propertyAttributes, List<IPropertySymbol> properties, string collectionName, int iterationCounter, bool isGroup = false)
    {
        var additionalPropIndexes = new List<int>();
        if (iterationCounter > 10)
        {
            context.ReportDiagnostic(Diagnostic.Create(TooManyIterationsError, typeAttribute.ApplicationSyntaxReference!.GetSyntax().GetLocation()));
            additionalPropIndexes.Add(0);
            return ("", additionalPropIndexes);
        }

        var builder = new StringBuilder();

        var type = (DataElementTypes)typeAttribute.ConstructorArguments[0].Value!;

        if (isGroup)
        {
            type &= ~DataElementTypes.Grouped;

            // Grouped element is missing the element own type. Return and add error diagnostic
            if (type == 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(GroupedMissingDefinitionError, typeAttribute.ApplicationSyntaxReference!.GetSyntax().GetLocation(), currentProp.Name));
                additionalPropIndexes.Add(0);
                return ("", additionalPropIndexes);
            }
        }

        switch (type)
        {
            case DataElementTypes.Value:
                builder.Append(GenerateValueRecord(currentProp, typeAttribute, propertyAttributes, collectionName));
                builder.AppendLine();
                additionalPropIndexes.Add(0);
                break;
            case DataElementTypes.KeyValue:
                builder.Append(GenerateKeyValueRecord(currentProp, typeAttribute, propertyAttributes, collectionName));
                builder.AppendLine();
                additionalPropIndexes.Add(0);
                break;
            case DataElementTypes.KeyValueUnit:
                builder.Append(GenerateKeyValueUnitRecord(context, currentProp, typeAttribute, propertyAttributes, collectionName));
                builder.AppendLine();
                additionalPropIndexes.Add(0);
                break;
            case DataElementTypes.Tooltip:
                builder.Append(GenerateTooltipRecord(context, currentProp, typeAttribute, propertyAttributes, collectionName));
                builder.AppendLine();
                additionalPropIndexes.Add(0);
                break;
            case DataElementTypes.FormattedText:
                builder.Append(GenerateFormattedTextRecord(context, currentProp, typeAttribute, propertyAttributes, collectionName));
                builder.AppendLine();
                additionalPropIndexes.Add(0);
                break;
            case { } when (type & DataElementTypes.Grouped) == DataElementTypes.Grouped:
                var (code, additionalIndexes) = GenerateGroupedRecord(context, typeAttribute, properties, collectionName, iterationCounter);
                builder.Append(code);
                builder.AppendLine();
                additionalPropIndexes.AddRange(additionalIndexes);
                break;
            default:
                context.ReportDiagnostic(Diagnostic.Create(InvalidTypeEnumError, Location.None, currentProp.Name));
                break;
        }

        return (builder.ToString(), additionalPropIndexes);
    }

    private static (string code, List<int> additionalIndexes) GenerateGroupedRecord(SourceProductionContext context, AttributeData typeAttr, List<IPropertySymbol> properties, string collectionName, int iterationCounter)
    {
        var groupName = (string?)typeAttr.NamedArguments.First(arg => arg.Key == "GroupKey").Value.Value;

        if (string.IsNullOrWhiteSpace(groupName))
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingAttributeError, typeAttr.ApplicationSyntaxReference!.GetSyntax().GetLocation(), "GroupKey", "GroupedDataElement"));
            return (string.Empty, new List<int>() { 0 });
        }

        var builder = new StringBuilder();

        builder.Append($@"{Indentation}var {groupName}List = new List<IDataElement>();");
        builder.AppendLine();

        var groupProperties = properties.Where(prop => prop.GetAttributes().Any(attribute => attribute.AttributeClass!.Name.Contains("DataElementType") && ((DataElementTypes)attribute.ConstructorArguments[0].Value!).HasFlag(DataElementTypes.Grouped) && attribute.NamedArguments.Any(arg => arg.Key == "GroupKey" && (arg.Value.Value?.Equals(groupName) ?? false)))).ToList();

        var indexList = groupProperties.Select(singleProperty => properties.IndexOf(singleProperty)).ToList();

        while (groupProperties.Any())
        {
            var currentGroupProp = groupProperties.First();
            var currentGroupPropertyAttributes = currentGroupProp.GetAttributes();

            //exclude the group attribute, to avoid infinite recursion. Need to find a better way to skip the Group type.
            var typeAttribute = currentGroupPropertyAttributes.FirstOrDefault();
            if (typeAttribute is not null)
            {
                if (typeAttribute.ConstructorArguments.IsEmpty)
                {
                    context.ReportDiagnostic(Diagnostic.Create(InvalidTypeEnumError, Location.None, currentGroupProp.Name));
                    groupProperties.RemoveAt(0);
                    continue;
                }

                var (code, additionalIndexes) = GenerateCode(context, typeAttribute, currentGroupProp, currentGroupPropertyAttributes, groupProperties, $"{groupName}List", iterationCounter++, true);
                builder.Append(code);
                foreach (var index in additionalIndexes.OrderByDescending(x => x))
                {
                    groupProperties.RemoveAt(index);
                }
            }
        }

        builder.Append($@"{IfIndentation}if ({groupName}List.Count > 0)");
        builder.AppendLine();
        builder.Append(@$"{Indentation}{collectionName}.Add(new GroupedDataElement(""ShipStats_{groupName}"", {groupName}List));");

        return (builder.ToString(), indexList);
    }

    private static string GenerateFormattedTextRecord(SourceProductionContext context, IPropertySymbol property, AttributeData typeAttribute, ImmutableArray<AttributeData> propertyAttributes, string collectionName)
    {
        var name = property.Name;
        var valuesProperty = (string?)typeAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "ValuesPropertyName").Value.Value;
        if (string.IsNullOrWhiteSpace(valuesProperty))
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingAttributeError, typeAttribute.ApplicationSyntaxReference!.GetSyntax().GetLocation(), "ValuesPropertyName", "TooltipDataElement"));
            return string.Empty;
        }

        var isKeyLocalization = (bool?)typeAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "IsValueLocalizationKey").Value.Value ?? false;
        var isListLocalization = (bool?)typeAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "ArePropertyNameValuesKeys").Value.Value ?? false;

        var isKeyAppLocalization = (bool?)typeAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "IsValueAppLocalization").Value.Value ?? false;
        var isListAppLocalization = (bool?)typeAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "IsPropertyNameValuesAppLocalization").Value.Value ?? false;

        var filter = GetFilterAttributeData(property.Name, propertyAttributes);
        var builder = new StringBuilder();
        builder.Append(filter);
        builder.AppendLine();
        builder.Append($@"{Indentation}{collectionName}.Add(new FormattedTextDataElement({name}, {valuesProperty}, {isKeyLocalization.ToString().ToLower()}, {isKeyAppLocalization.ToString().ToLower()}, {isListLocalization.ToString().ToLower()}, {isListAppLocalization.ToString().ToLower()}));");
        return builder.ToString();
    }

    private static string GenerateTooltipRecord(SourceProductionContext context, IPropertySymbol property, AttributeData typeAttribute, ImmutableArray<AttributeData> propertyAttributes, string collectionName)
    {
        var name = property.Name;
        var propertyProcessingAddition = GetPropertyAddition(property);

        var tooltip = (string?)typeAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "TooltipKey").Value.Value;
        if (string.IsNullOrWhiteSpace(tooltip))
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingAttributeError, typeAttribute.ApplicationSyntaxReference!.GetSyntax().GetLocation(), "TooltipKey", "TooltipDataElement"));
            return string.Empty;
        }

        var unit = (string?)typeAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "UnitKey").Value.Value ?? "";
        if (!string.IsNullOrWhiteSpace(unit))
        {
            unit = "Unit_" + unit;
        }

        var localizationKey = (string?)typeAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "NameLocalizationKey").Value.Value ?? name;

        var filter = GetFilterAttributeData(property.Name, propertyAttributes);

        var builder = new StringBuilder();
        builder.Append(filter);
        builder.AppendLine();
        builder.Append($@"{Indentation}{collectionName}.Add(new TooltipDataElement(""ShipStats_{localizationKey}"", {name}{propertyProcessingAddition}, ""ShipStats_{tooltip}"", ""{unit}""));");
        return builder.ToString();
    }

    private static string GenerateKeyValueUnitRecord(SourceProductionContext context, IPropertySymbol property, AttributeData typeAttribute, ImmutableArray<AttributeData> propertyAttributes, string collectionName)
    {
        var name = property.Name;
        var propertyProcessingAddition = GetPropertyAddition(property);

        var unit = (string?)typeAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "UnitKey").Value.Value;
        if (string.IsNullOrWhiteSpace(unit))
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingAttributeError, typeAttribute.ApplicationSyntaxReference!.GetSyntax().GetLocation(), "UnitKey", "KeyValueUnitDataElement"));
            return string.Empty;
        }

        var localizationKey = (string?)typeAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "NameLocalizationKey").Value.Value ?? name;

        var filter = GetFilterAttributeData(property.Name, propertyAttributes);

        var builder = new StringBuilder();
        builder.Append(filter);
        builder.AppendLine();
        builder.Append($@"{Indentation}{collectionName}.Add(new KeyValueUnitDataElement(""ShipStats_{localizationKey}"", {name}{propertyProcessingAddition}, ""Unit_{unit}""));");
        return builder.ToString();
    }

    private static string GenerateKeyValueRecord(IPropertySymbol property, AttributeData typeAttribute, ImmutableArray<AttributeData> propertyAttributes, string collectionName)
    {
        var name = property.Name;
        var propertyProcessingAddition = GetPropertyAddition(property);
        var filter = GetFilterAttributeData(property.Name, propertyAttributes);

        var isKeyLocalization = (bool?)typeAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "IsValueLocalizationKey").Value.Value ?? false;
        var isKeyAppLocalization = (bool?)typeAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "IsValueAppLocalization").Value.Value ?? false;

        var localizationKey = (string?)typeAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "NameLocalizationKey").Value.Value ?? name;

        var builder = new StringBuilder();
        builder.Append(filter);
        builder.AppendLine();
        builder.Append($@"{Indentation}{collectionName}.Add(new KeyValueDataElement(""ShipStats_{localizationKey}"", {name}{propertyProcessingAddition}, {isKeyLocalization.ToString().ToLower()}, {isKeyAppLocalization.ToString().ToLower()}));");
        return builder.ToString();
    }

    private static string GenerateValueRecord(IPropertySymbol property, AttributeData typeAttribute, ImmutableArray<AttributeData> propertyAttributes, string collectionName)
    {
        var propertyProcessingAddition = GetPropertyAddition(property);
        var filter = GetFilterAttributeData(property.Name, propertyAttributes);

        var isKeyLocalization = (bool?)typeAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "IsValueLocalizationKey").Value.Value ?? false;
        var isKeyAppLocalization = (bool?)typeAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "IsValueAppLocalization").Value.Value ?? false;

        var builder = new StringBuilder();
        builder.Append(filter);
        builder.AppendLine();
        builder.Append($@"{Indentation}{collectionName}.Add(new ValueDataElement({property.Name}{propertyProcessingAddition}, {isKeyLocalization.ToString().ToLower()}, {isKeyAppLocalization.ToString().ToLower()}));");
        return builder.ToString();
    }

    private static string GetPropertyAddition(IPropertySymbol property)
    {
        var propertyProcessingAddition = string.Empty;
        if (!property.Type.Name.Equals("string", StringComparison.InvariantCultureIgnoreCase))
        {
            propertyProcessingAddition = ".ToString()";
        }
        else if (property.NullableAnnotation == NullableAnnotation.Annotated)
        {
            propertyProcessingAddition = " ?? \"null\"";
        }

        return propertyProcessingAddition;
    }

    private static string GetFilterString(string propertyName, bool filterEnabled, string filterName)
    {
        var filter = string.Empty;
        if (!filterEnabled)
        {
            return filter;
        }

        if (string.IsNullOrWhiteSpace(filterName))
        {
            return @$"{IfIndentation}if ({DataContainerBaseName}.ShouldAdd({propertyName}))";
        }

        filter = @$"{IfIndentation}if ({filterName}({propertyName}))";
        return filter;
    }

    private static string GetFilterAttributeData(string propertyName, ImmutableArray<AttributeData> propertyAttributes)
    {
        var attribute = propertyAttributes.FirstOrDefault(attribute => attribute.AttributeClass!.Name.Contains("DataElementFilteringAttribute"));

        // if there is no attribute, returns active filter, no name for custom filter.
        if (attribute is null)
        {
            return GetFilterString(propertyName, true, "");
        }

        var filterEnabled = (bool)attribute.ConstructorArguments[0].Value!;
        string filterName = string.Empty;
        if (attribute.ConstructorArguments.Length > 1)
        {
            filterName = attribute.ConstructorArguments[1].Value?.ToString() ?? string.Empty;
        }

        return GetFilterString(propertyName, filterEnabled, filterName);
    }
}
