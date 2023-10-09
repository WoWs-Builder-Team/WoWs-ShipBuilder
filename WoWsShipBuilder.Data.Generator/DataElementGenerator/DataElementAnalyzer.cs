using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using WoWsShipBuilder.Data.Generator.DataElementGenerator.Model;
using WoWsShipBuilder.Data.Generator.Utilities;

namespace WoWsShipBuilder.Data.Generator.DataElementGenerator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DataElementAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Rules.InvalidDataElementTypeRule,
        Rules.MissingSecondaryDataElementTypeRule,
        Rules.GroupKeyMissingRule,
        Rules.MissingAttributeParametersRule,
        Rules.IncompatibleAttributeParametersRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeDataElementProperty, SymbolKind.Property);
    }

    private static void AnalyzeDataElementProperty(SymbolAnalysisContext context)
    {
        var propertySymbol = (IPropertySymbol)context.Symbol;
        var dataElementAttribute = propertySymbol.FindAttributeOrDefault($"{AttributeHelper.AttributeNamespace}.{AttributeHelper.DataElementTypeAttributeName}");
        if (dataElementAttribute is null)
        {
            return;
        }

        var dataElementType = (DataElementTypes)dataElementAttribute.ConstructorArguments[0].Value!;
        var propertyData = ExtractPropertyData(propertySymbol, dataElementType, dataElementAttribute);
        context.CancellationToken.ThrowIfCancellationRequested();

        if ((dataElementType & DataElementTypes.Grouped) == DataElementTypes.Grouped)
        {
            CheckGroupedDataElement(context, propertySymbol, dataElementType, dataElementAttribute);
        }
        else if (propertyData.DisplayOptions.GroupKey is not null)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rules.IncompatibleAttributeParametersRule, propertySymbol.Locations[0], "GroupKey"));
        }

        context.CancellationToken.ThrowIfCancellationRequested();
        switch (dataElementType & ~DataElementTypes.Grouped)
        {
            case DataElementTypes.KeyValue:
                CheckKeyValueDataElement(context, propertySymbol, propertyData);
                break;
            case DataElementTypes.KeyValueUnit:
                CheckKeyValueUnitDataElement(context, propertySymbol, propertyData);
                break;
            case DataElementTypes.Value:
                CheckValueDataElement(context, propertySymbol, propertyData);
                break;
            case DataElementTypes.Tooltip:
                CheckTooltipDataElement(context, propertySymbol, propertyData);
                break;
            case DataElementTypes.FormattedText:
                CheckFormattedTextDataElement(context, propertySymbol, propertyData);
                break;
            default:
                if ((dataElementType & DataElementTypes.Grouped) != DataElementTypes.Grouped)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rules.InvalidDataElementTypeRule, propertySymbol.Locations[0], dataElementType));
                }

                break;
        }
    }

    private static void CheckGroupedDataElement(SymbolAnalysisContext context, ISymbol propertySymbol, DataElementTypes dataElementType, AttributeData dataElementAttribute)
    {
        if ((dataElementType & ~DataElementTypes.Grouped) == 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rules.MissingSecondaryDataElementTypeRule, propertySymbol.Locations[0]));
        }

        if (dataElementAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "GroupKey").Value.Value is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rules.GroupKeyMissingRule, propertySymbol.Locations[0]));
        }
    }

    private static void CheckKeyValueDataElement(SymbolAnalysisContext context, ISymbol propertySymbol, SinglePropertyData propertyData)
    {
        var incompatibleParameters = new List<string>();

        if (propertyData.FormattedTextData.ArgumentsCollectionName is not null)
        {
            incompatibleParameters.Add("ArgumentsCollectionName");
        }

        if (propertyData.FormattedTextData.ArgumentsTextKind != TextKind.Plain)
        {
            incompatibleParameters.Add("ArgumentsTextKind");
        }

        if (propertyData.DisplayOptions.UnitKey is not null)
        {
            incompatibleParameters.Add("UnitKey");
        }

        if (propertyData.DisplayOptions.TooltipKey is not null)
        {
            incompatibleParameters.Add("TooltipKey");
        }

        if (incompatibleParameters.Count > 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rules.IncompatibleAttributeParametersRule, propertySymbol.Locations[0], string.Join(", ", incompatibleParameters)));
        }
    }

    private static void CheckKeyValueUnitDataElement(SymbolAnalysisContext context, ISymbol propertySymbol, SinglePropertyData propertyData)
    {
        var errors = new List<string>();

        if (propertyData.FormattedTextData.ArgumentsCollectionName is not null)
        {
            errors.Add("ArgumentsCollectionName");
        }

        if (propertyData.FormattedTextData.ArgumentsTextKind != TextKind.Plain)
        {
            errors.Add("ArgumentsTextKind");
        }

        if (propertyData.DisplayOptions.TooltipKey is not null)
        {
            errors.Add("TooltipKey");
        }

        if (propertyData.DisplayOptions.ValueTextKind != TextKind.Plain)
        {
            errors.Add("ValueTextKind");
        }

        if (errors.Count > 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rules.IncompatibleAttributeParametersRule, propertySymbol.Locations[0], string.Join(", ", errors)));
        }

        if (string.IsNullOrEmpty(propertyData.DisplayOptions.UnitKey))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rules.MissingAttributeParametersRule, propertySymbol.Locations[0], "UnitKey"));
        }
    }

    private static void CheckValueDataElement(SymbolAnalysisContext context, ISymbol propertySymbol, SinglePropertyData propertyData)
    {
        var incompatibleParameters = new List<string>();

        if (propertyData.FormattedTextData.ArgumentsCollectionName is not null)
        {
            incompatibleParameters.Add("ArgumentsCollectionName");
        }

        if (propertyData.FormattedTextData.ArgumentsTextKind != TextKind.Plain)
        {
            incompatibleParameters.Add("ArgumentsTextKind");
        }

        if (propertyData.DisplayOptions.UnitKey is not null)
        {
            incompatibleParameters.Add("UnitKey");
        }

        if (propertyData.DisplayOptions.TooltipKey is not null)
        {
            incompatibleParameters.Add("TooltipKey");
        }

        if (!propertyData.DisplayOptions.LocalizationKey.Equals(propertySymbol.Name))
        {
            incompatibleParameters.Add("LocalizationKeyOverride");
        }

        if (incompatibleParameters.Count > 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rules.IncompatibleAttributeParametersRule, propertySymbol.Locations[0], string.Join(", ", incompatibleParameters)));
        }
    }

    private static void CheckTooltipDataElement(SymbolAnalysisContext context, ISymbol propertySymbol, SinglePropertyData propertyData)
    {
        var errors = new List<string>();

        if (propertyData.FormattedTextData.ArgumentsCollectionName is not null)
        {
            errors.Add("ArgumentsCollectionName");
        }

        if (propertyData.FormattedTextData.ArgumentsTextKind != TextKind.Plain)
        {
            errors.Add("ArgumentsTextKind");
        }

        if (propertyData.DisplayOptions.ValueTextKind != TextKind.Plain)
        {
            errors.Add("ValueTextKind");
        }

        if (errors.Count > 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rules.IncompatibleAttributeParametersRule, propertySymbol.Locations[0], string.Join(", ", errors)));
        }

        if (string.IsNullOrEmpty(propertyData.DisplayOptions.TooltipKey))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rules.MissingAttributeParametersRule, propertySymbol.Locations[0], "TooltipKey"));
        }
    }

    private static void CheckFormattedTextDataElement(SymbolAnalysisContext context, ISymbol propertySymbol, SinglePropertyData propertyData)
    {
        var errors = new List<string>();

        if (propertyData.DisplayOptions.UnitKey is not null)
        {
            errors.Add("UnitKey");
        }

        if (propertyData.DisplayOptions.TooltipKey is not null)
        {
            errors.Add("TooltipKey");
        }

        if (!propertyData.DisplayOptions.LocalizationKey.Equals(propertySymbol.Name))
        {
            errors.Add("LocalizationKeyOverride");
        }

        if (errors.Count > 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rules.IncompatibleAttributeParametersRule, propertySymbol.Locations[0], string.Join(", ", errors)));
        }

        if (string.IsNullOrEmpty(propertyData.FormattedTextData.ArgumentsCollectionName))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rules.MissingAttributeParametersRule, propertySymbol.Locations[0], "ArgumentsCollectionName"));
        }
    }

    private static SinglePropertyData ExtractPropertyData(IPropertySymbol propertySymbol, DataElementTypes dataElementType, AttributeData dataElementAttribute)
    {
        return new(propertySymbol.Name, propertySymbol.Type.SpecialType == SpecialType.System_String, propertySymbol.NullableAnnotation == NullableAnnotation.Annotated, dataElementType, PropertyHelper.ExtractDisplayOptions(propertySymbol, dataElementAttribute), PropertyHelper.ExtractFilterOptions(propertySymbol), PropertyHelper.ExtractFormattedTextOptions(dataElementAttribute), 0);
    }
}
