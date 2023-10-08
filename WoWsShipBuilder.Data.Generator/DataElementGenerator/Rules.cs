using Microsoft.CodeAnalysis;

namespace WoWsShipBuilder.Data.Generator.DataElementGenerator;

internal static class Rules
{
    public const string DataElementCategory = "DataElement";

    public const string InvalidDataElementTypeId = "SB0001";

    public const string MissingSecondaryDataElementTypeId = "SB0002";

    public const string GroupKeyMissingId = "SB1001";

    public const string MissingAttributeParametersId = "SB1002";

    public const string IncompatibleAttributeParametersId = "SB1003";

    public static readonly DiagnosticDescriptor InvalidDataElementTypeRule = new(
        id: InvalidDataElementTypeId,
        title: "Invalid DataElementType",
        messageFormat: "The specified DataElementType is not valid: {0}",
        category: DataElementCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingSecondaryDataElementTypeRule = new(
        id: MissingSecondaryDataElementTypeId,
        title: "Missing DataElementType specification",
        messageFormat: "When using DataElementTypes.Grouped, a second type must be specified",
        category: DataElementCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GroupKeyMissingRule = new(
        id: GroupKeyMissingId,
        title: "Missing GroupKey",
        messageFormat: "When using DataElementTypes.Grouped, a GroupKey must be specified",
        category: DataElementCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingAttributeParametersRule = new(
        id: MissingAttributeParametersId,
        title: "Missing attribute parameters",
        messageFormat: "The following attribute parameters are required with the specified DataElementType but not specified: {0}",
        category: DataElementCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor IncompatibleAttributeParametersRule = new(
        id: IncompatibleAttributeParametersId,
        title: "Incompatible attribute parameters",
        messageFormat: "The following attribute parameters are incompatible with the specified DataElementType: {0}",
        category: DataElementCategory,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
