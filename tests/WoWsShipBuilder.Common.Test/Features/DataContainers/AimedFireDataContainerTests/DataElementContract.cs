using System.Reflection;
using FluentAssertions;
using WoWsShipBuilder.Features.DataContainers;

namespace WoWsShipBuilder.Test.Features.DataContainers.AimedFireDataContainerTests;

[TestFixture]
public class DataElementContract
{
    /// <summary>
    /// DataContainerBase.ShouldAdd only understands string, decimal, int and (decimal, decimal). A property of any
    /// other type - float, double, bool - fails the generated filter and its row is dropped without any error, so the
    /// stat would simply never appear in the UI.
    /// </summary>
    [Test]
    public void EveryDataElementProperty_UsesATypeShouldAddUnderstands()
    {
        var supportedTypes = new[] { typeof(string), typeof(decimal), typeof(int) };

        var unsupported = typeof(AimedFireDataContainer).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CustomAttributes.Any(attribute => attribute.AttributeType.Name.Equals("DataElementTypeAttribute", StringComparison.Ordinal)))
            .Where(property => !supportedTypes.Contains(property.PropertyType))
            .Select(property => $"{property.Name} ({property.PropertyType.Name})")
            .ToList();

        unsupported.Should().BeEmpty("DataContainerBase.ShouldAdd silently hides every data element whose type it does not understand");
    }
}
