using System;
using Splat;

namespace WoWsShipBuilder.Core.Extensions
{
    public static class LocatorExtensions
    {
        public static T GetServiceSafe<T>(this IReadonlyDependencyResolver resolver)
        {
            var service = resolver.GetService<T>();
            return service ?? throw new InvalidOperationException("Resolver returned null.");
        }
    }
}
