using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace WoWsShipBuilder.Data.Generator.Internals;

internal static class SymbolExtensions
{
    public static AttributeData? FindAttributeOrDefault(this ISymbol symbol, string fullAttributeName)
    {
        return symbol.GetAttributes().FirstOrDefault(attribute => attribute.AttributeClass?.ToDisplayString().Equals(fullAttributeName, StringComparison.Ordinal) == true);
    }

    public static AttributeData FindAttribute(this ISymbol symbol, string fullAttributeName)
    {
        return symbol.FindAttributeOrDefault(fullAttributeName) ?? throw new KeyNotFoundException($"No attribute found with name {fullAttributeName}.");
    }

    public static bool HasAttributeWithFullName(this ISymbol symbol, string fullAttributeName)
    {
        return symbol.FindAttributeOrDefault(fullAttributeName) != null;
    }

    public static bool HasInterface(this INamedTypeSymbol symbol, INamedTypeSymbol interfaceType)
    {
        return symbol.AllInterfaces.Contains(interfaceType, SymbolEqualityComparer.Default);
    }

    public static bool NamespaceContains(this INamedTypeSymbol symbol, string search)
    {
        return symbol.ContainingNamespace.ToDisplayString().IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static string ToLowerString(this bool value) => value.ToString().ToLowerInvariant();

}
