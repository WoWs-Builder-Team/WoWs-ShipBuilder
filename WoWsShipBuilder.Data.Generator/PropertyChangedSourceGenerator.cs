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
using WoWsShipBuilder.Data.Generator.Internals;

namespace WoWsShipBuilder.Data.Generator;

[Generator]
public class PropertyChangedSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var viewModels = context.SyntaxProvider.CreateSyntaxProvider(ViewModelFilter, GetViewModelOrNull)
            .Where(type => type is not null)
            .Select(GetObservableFields!)
            .Where(result => result.Items.Count > 0)
            .Collect();
        context.RegisterSourceOutput(viewModels, GenerateCode);
    }

    private static bool ViewModelFilter(SyntaxNode syntaxNode, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (syntaxNode is not ClassDeclarationSyntax classDeclaration)
        {
            return false;
        }

        // var validBaseType = classDeclaration.BaseList?.Types.Any(baseType => baseType.ToString().Contains("ViewModelBase") || baseType.ToString().Contains(nameof(INotifyPropertyChanged))) ?? false;
        return classDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword));
    }

    private static ITypeSymbol? GetViewModelOrNull(GeneratorSyntaxContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;
        return context.SemanticModel.GetDeclaredSymbol(typeDeclaration);
    }

    private static SourceGenFilterResult GetObservableFields(ITypeSymbol symbol, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        string className = symbol.Name;
        string classNamespace = symbol.ContainingNamespace.ToDisplayString();
        List<IFieldSymbol> fields = symbol.GetMembers().OfType<IFieldSymbol>().Where(field => field.GetAttributes().Any(attr => attr.AttributeClass!.Name == "ObservableAttribute")).ToList();
        return new(className, classNamespace, fields);
    }

    private static void GenerateCode(SourceProductionContext context, ImmutableArray<SourceGenFilterResult> viewmodels)
    {
        var logMessages = new StringBuilder();
        foreach (var viewmodel in viewmodels)
        {
            var classStart = $@"
using ReactiveUI;

namespace {viewmodel.ElementNamespace};

#nullable enable
public partial class {viewmodel.ElementName}
{{
";
            const string classEnd = @"
}
#nullable restore
";

            var classBuilder = new StringBuilder(classStart);
            foreach (var field in viewmodel.Items)
            {
                var propertyName = char.ToUpper(field.Name.First()) + field.Name.Substring(1);
                var fieldAttribute = field.GetAttributes().First(a => a.AttributeClass!.Name == "ObservableAttribute");
                var setterVisibility = (Visibility)(fieldAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "SetterVisibility").Value.Value ?? Visibility.Public);
                var setterVisibilityString = setterVisibility switch
                {
                    Visibility.Public => string.Empty,
                    Visibility.Protected => "protected ",
                    Visibility.Internal => "internal ",
                    Visibility.Private => "private ",
                    _ => throw new InvalidOperationException(),
                };
                logMessages.AppendLine("#4");
                classBuilder.AppendLine($@"
    public {field.Type} {propertyName}
    {{
        get => this.{field.Name};
        {setterVisibilityString}set => this.RaiseAndSetIfChanged(ref this.{field.Name}, value);
    }}");
            }

            classBuilder.AppendLine(classEnd);
            context.AddSource($"{viewmodel.ElementName}.g.cs", SourceText.From(classBuilder.ToString(), Encoding.UTF8));
        }
    }

    private record struct SourceGenFilterResult(string ElementName, string ElementNamespace, List<IFieldSymbol> Items);
}
