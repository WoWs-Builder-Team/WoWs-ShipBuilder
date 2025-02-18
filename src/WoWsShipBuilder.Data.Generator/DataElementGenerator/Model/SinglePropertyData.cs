namespace WoWsShipBuilder.Data.Generator.DataElementGenerator.Model;

internal sealed record SinglePropertyData(string Name, bool IsString, bool IsNullable, DataElementTypes DataElementType, PropertyDisplayOptions DisplayOptions, PropertyFilter PropertyFilter, FormattedTextData FormattedTextData, int DeclarationIndex);
