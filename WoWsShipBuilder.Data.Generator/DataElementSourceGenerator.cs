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
using WoWsShipBuilder.Data.Generator.Attributes;
using WoWsShipBuilder.Data.Generator.Internals;

namespace WoWsShipBuilder.Data.Generator;

[Generator]
public class DataElementSourceGenerator : IIncrementalGenerator
{
    private const string BaseInterfaceName = "IDataUi";
    private const string GeneratedMethodName = "UpdateDataElements";
    private const string DataElementCollectionName = "DataElements";
    private const string Indentation = "        ";

    private static readonly DiagnosticDescriptor MissingAttributeError = new DiagnosticDescriptor(id: "DTELMGEN001",
                                                                             title: "A required secondary attribute is missing",
                                                                             messageFormat: "Couldn't find the required attribute {0} for the DataElement type {1}",
                                                                             category: "DataElementGenerator",
                                                                             DiagnosticSeverity.Error,
                                                                             isEnabledByDefault: true);


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var dataClasses = context.SyntaxProvider.CreateSyntaxProvider(IsDataUiRecord, GetRecordTypeOrNull)
            .Where(type => type is not null)
            .Collect();

        // Uncomment to generate attribute classes instead of using them from a separate dependency
        // context.RegisterPostInitializationOutput(GenerateFixedCode);
        context.RegisterSourceOutput(dataClasses, GenerateCode!);
    }

    private static bool IsDataUiRecord(SyntaxNode syntaxNode, CancellationToken token)
    {
        if (syntaxNode is not RecordDeclarationSyntax recordSyntax)
        {
            return false;
        }

        return recordSyntax.Modifiers.ToString().Contains("partial") && (recordSyntax.BaseList?.Types.ToString().Contains(BaseInterfaceName) ?? false);
    }

    private static ITypeSymbol? GetRecordTypeOrNull(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var recordSyntax = (RecordDeclarationSyntax)context.Node;
        return context.SemanticModel.GetDeclaredSymbol(recordSyntax);
    }

    private static void GenerateFixedCode(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource("DataElementTypes.g.cs", SourceText.From(AttributeGenerator.DataElementTypesEnum, Encoding.UTF8));
        context.AddSource("DataElementTypeAttribute.g.cs", SourceText.From(AttributeGenerator.DataElementTypeAttribute, Encoding.UTF8));
        context.AddSource("DataElementTooltipAttribute.g.cs", SourceText.From(AttributeGenerator.DataElementTooltipAttribute, Encoding.UTF8));
        context.AddSource("DataElementGroupAttribute.g.cs", SourceText.From(AttributeGenerator.DataElementGroupAttribute, Encoding.UTF8));
        context.AddSource("DataElementUnitAttribute.g.cs", SourceText.From(AttributeGenerator.DataElementUnitAttribute, Encoding.UTF8));
    }

    private static void GenerateCode(SourceProductionContext context, ImmutableArray<ITypeSymbol> dataRecords)
    {
        foreach (var dataRecord in dataRecords)
        {
            var properties = dataRecord.GetMembers().OfType<IPropertySymbol>().Where(prop => prop.GetAttributes().Any(attr => !attr.AttributeClass!.Name.Contains("Ignore"))).ToList();
            var classStart = $@"
using System;
using System.Collections.Generic;
using WoWsShipBuilder.Core.DataUI.DataElements;

namespace {dataRecord.ContainingNamespace.ToDisplayString()};
#nullable enable
public partial record {dataRecord.Name}
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

                var typeAttribute = propertyAttributes.FirstOrDefault(attr => attr.AttributeClass!.Name == nameof(AttributeGenerator.DataElementTypeAttribute));
                if (typeAttribute is not null)
                {
                    var type = (DataElementTypes)typeAttribute.ConstructorArguments[0].Value!;
                    var (code, additionalIndexes) = GenerateCode(context, type, prop, propertyAttributes, properties, DataElementCollectionName);
                    builder.Append(code);
                    builder.AppendLine();

                    foreach (var index in additionalIndexes.OrderByDescending(x => x))
                    {
                        properties.RemoveAt(index);
                    }
                }
            }

            builder.Append(classEnd);
            context.AddSource($"{dataRecord.Name}.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
        }
    }

    private static (string code, List<int> additionalIndexes) GenerateCode(SourceProductionContext context, DataElementTypes type, IPropertySymbol currentProp, ImmutableArray<AttributeData> propertyAttributes, List<IPropertySymbol> properties, string collectionName)
    {
        var builder = new StringBuilder();
        var additionalPropIndexes = new List<int>();
        switch (type)
        {
            case DataElementTypes.Value:
                builder.Append(GenerateValueRecord(currentProp, collectionName));
                builder.AppendLine();
                additionalPropIndexes.Add(0);
                break;
            case DataElementTypes.KeyValue:
                builder.Append(GenerateKeyValueRecord(currentProp, collectionName));
                builder.AppendLine();
                additionalPropIndexes.Add(0);
                break;
            case DataElementTypes.KeyValueUnit:
                builder.Append(GenerateKeyValueUnitRecord(context, currentProp, propertyAttributes, collectionName));
                builder.AppendLine();
                additionalPropIndexes.Add(0);
                break;
            case DataElementTypes.Tooltip:
                builder.Append(GenerateTooltipRecord(context, currentProp, propertyAttributes, collectionName));
                builder.AppendLine();
                additionalPropIndexes.Add(0);
                break;
            case DataElementTypes.Grouped:
                var (code, additionalIndexes) = GenerateGroupedRecord(context, propertyAttributes, properties, collectionName);
                builder.Append(code);
                builder.AppendLine();
                additionalPropIndexes.AddRange(additionalIndexes);
                break;
        }

        return (builder.ToString(), additionalPropIndexes);
    }

    private static (string code, List<int> additionalIndexes) GenerateGroupedRecord(SourceProductionContext context, ImmutableArray<AttributeData> propertyAttributes, List<IPropertySymbol> properties, string collectionName)
    {
        var tooltipAttribute = propertyAttributes.FirstOrDefault(x => x.AttributeClass!.Name.Contains("DataElementGroupAttribute"));
        if (tooltipAttribute is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingAttributeError, Location.None, "DataElementGroupAttribute", "GroupedDataElement"));
            return (string.Empty, new List<int>());
        }
        var groupName = (string) tooltipAttribute.ConstructorArguments[0].Value!;

        var builder = new StringBuilder();

        builder.Append($@"{Indentation}var {groupName}List = new List<IDataElement>();");
        builder.AppendLine();

        var groupProperties = properties.Where(prop => prop.GetAttributes().Any(attribute => attribute.AttributeClass!.Name.Contains("DataElementGroupAttribute") && attribute.ConstructorArguments[0].Value!.Equals(groupName))).ToList();

        var indexList = groupProperties.Select(singleProperty => properties.IndexOf(singleProperty)).ToList();

        while (groupProperties.Any())
        {
            var currentGroupProp = groupProperties.First();
            var currentGroupPropertyAttributes = currentGroupProp.GetAttributes();

            //exclude the group attribute, to avoid infinite recursion. Need to find a better way to skip the Group type.
            var typeAttribute = currentGroupPropertyAttributes.LastOrDefault(attr => attr.AttributeClass!.Name == nameof(AttributeGenerator.DataElementTypeAttribute));
            if (typeAttribute is not null)
            {
                var type = (DataElementTypes) typeAttribute.ConstructorArguments[0].Value!;
                var (code, additionalIndexes) = GenerateCode(context, type, currentGroupProp, currentGroupPropertyAttributes, groupProperties, $"{groupName}List");
                builder.Append(code);
                foreach (var index in additionalIndexes.OrderByDescending(x => x))
                {
                    groupProperties.RemoveAt(index);
                }
            }
        }

        builder.Append(@$"{Indentation}{collectionName}.Add(new GroupedDataElement(""{groupName}"", {groupName}List));");

        return (builder.ToString(), indexList);
    }

    private static string GenerateTooltipRecord(SourceProductionContext context, IPropertySymbol property, ImmutableArray<AttributeData> propertyAttributes, string collectionName)
    {
        var name = property.Name;
        var propertyProcessingAddition = GetPropertyAddition(property);
        var tooltipAttribute = propertyAttributes.FirstOrDefault(x => x.AttributeClass!.Name.Contains("DataElementTooltipAttribute"));
        if (tooltipAttribute is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingAttributeError, Location.None, "DataElementTooltipAttribute", "TooltipDataElement"));
            return string.Empty;
        }
        var tooltip = (string) tooltipAttribute.ConstructorArguments[0].Value!;

        return $@"{Indentation}{collectionName}.Add(new TooltipDataElement(""{name}"", {name}{propertyProcessingAddition}, ""{tooltip}""));";
    }

    private static string GenerateKeyValueUnitRecord(SourceProductionContext context, IPropertySymbol property, ImmutableArray<AttributeData> propertyAttributes, string collectionName)
    {
        var name = property.Name;
        var propertyProcessingAddition = GetPropertyAddition(property);

        var unitAttribute = propertyAttributes.FirstOrDefault(x => x.AttributeClass!.Name.Contains("DataElementUnitAttribute"));
        if (unitAttribute is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingAttributeError, Location.None, "DataElementUnitAttribute", "KeyValueUnitDataElement"));
            return string.Empty;
        }
        var unit = (string) unitAttribute.ConstructorArguments[0].Value!;
        return $@"{Indentation}{collectionName}.Add(new KeyValueUnitDataElement(""{name}"", {name}{propertyProcessingAddition}, ""Unit_{unit}""));";
    }

    private static string GenerateKeyValueRecord(IPropertySymbol property, string collectionName)
    {
        var name = property.Name;
        var propertyProcessingAddition = GetPropertyAddition(property);
        return $@"{Indentation}{collectionName}.Add(new KeyValueDataElement(""{name}"", {name}{propertyProcessingAddition}));";
    }

    private static string GenerateValueRecord(IPropertySymbol property, string collectionName)
    {
        var propertyProcessingAddition = GetPropertyAddition(property);
        return $@"{Indentation}{collectionName}.Add(new ValueDataElement({property.Name}{propertyProcessingAddition}));";
    }

    private static string GetPropertyAddition(IPropertySymbol property)
    {
        var propertyProcessingAddition = string.Empty;
        if (!property.Type.Name.Equals("string", StringComparison.InvariantCultureIgnoreCase))
        {
            propertyProcessingAddition = ".ToString()";
        }
        return propertyProcessingAddition;
    }
}
