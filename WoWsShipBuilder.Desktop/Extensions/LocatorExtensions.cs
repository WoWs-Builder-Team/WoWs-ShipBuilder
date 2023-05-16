using System;
using Splat;

namespace WoWsShipBuilder.Desktop.Extensions;

public static class LocatorExtensions
{
    public static T GetRequiredService<T>(this IReadonlyDependencyResolver resolver)
    {
        var service = resolver.GetService<T>();
        return service ?? throw new InvalidOperationException("Resolver returned null.");
    }
}
