using System;
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
    public const string BaseInterfaceName = "IDataUi";
    public const string GeneratedMethodName = "UpdateDataElements";
    public const string DataElementCollectionName = "DataElements";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var dataClasses = context.SyntaxProvider.CreateSyntaxProvider(IsDataUiRecord, GetRecordTypeOrNull)
            .Where(type => type is not null)
            .Collect();

        // Uncomment to generate attribute classes instead of using them from a separate dependency
        context.RegisterPostInitializationOutput(GenerateFixedCode);
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
    }

    private static void GenerateCode(SourceProductionContext context, ImmutableArray<ITypeSymbol> dataRecords)
    {
        foreach (var dataRecord in dataRecords)
        {
            var properties = dataRecord.GetMembers().OfType<IPropertySymbol>().Where(prop => prop.GetAttributes().Any(attr => !attr.AttributeClass!.Name.Contains("Ignore"))).ToList();
            var methodStart = $@"
using System;
using WoWsShipBuilder.Core.DataUI.DataElements;

namespace {dataRecord.ContainingNamespace.ToDisplayString()};
public partial record {dataRecord.Name}
{{
    private void {GeneratedMethodName}()
    {{
        {DataElementCollectionName}.Clear();
";
            const string methodEnd = @"
    }
}
";
            var builder = new StringBuilder(methodStart);

            while (properties.Any())
            {
                var prop = properties.First();
                var propertyAttributes = prop.GetAttributes();

                var typeAttribute = propertyAttributes.FirstOrDefault(attr => attr.AttributeClass!.Name == nameof(AttributeGenerator.DataElementTypeAttribute));
                if (typeAttribute is not null)
                {
                    var type = (DataElementTypes)typeAttribute.ConstructorArguments[0].Value!;
                    switch (type)
                    {
                        case DataElementTypes.Value:
                            builder.Append(GenerateValueRecord(prop, propertyAttributes));
                            break;
                    }
                }

                properties.RemoveAt(0);
            }

            builder.Append(methodEnd);
            context.AddSource($"{dataRecord.Name}.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
        }
    }

    private static string GenerateValueRecord(IPropertySymbol property, ImmutableArray<AttributeData> attributes)
    {
        var propertyProcessingAddition = string.Empty;
        if (property.Type.Name.Equals("string", StringComparison.InvariantCultureIgnoreCase))
        {
            propertyProcessingAddition = ".ToString()";
        }
        return $@"{DataElementCollectionName}.Add(new ValueDataElement({property.Name}{propertyProcessingAddition}));";
    }
}
