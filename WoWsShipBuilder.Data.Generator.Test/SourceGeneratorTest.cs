using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace WoWsShipBuilder.Data.Generator.Test;

[TestFixture]
public class SourceGeneratorTest
{
    [Test]
    public void AllTest()
    {
        var code = @"using WoWsShipBuilder.DataElements.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;

namespace WoWsShipBuilder.Data.Generator.Test.TestStructures;

public partial record TestDataUi1 : ProjectileDataContainer
{
    [DataElementType(DataElementTypes.Value)]
    public string TestValue { get; init; } = default!;

    [DataElementType(DataElementTypes.KeyValue)]
    [DataElementVisibility(false)]
    public decimal TestKeyValue { get; init; }

    [DataElementType(DataElementTypes.KeyValue)]
    [DataElementVisibility(true, ""TestVisibility"")]
    public decimal TestVisibilityCustom { get; init; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = ""mm"")]
    public string TestKeyUnitValue { get; init; } = default!;

    [DataElementType(DataElementTypes.Tooltip, TooltipKey = ""testTooltip"")]
    public decimal TestTooltipValue { get; init; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = ""test1"")]
    public string TestGroup1 { get; init; } = default!;

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = ""test1"")]
    public string Test2Group1 { get; init; } = default!;

    public bool TestVisibility(object value)
    {
        return true;
    }
}
";
        _ = VerifyGenerator(code);
    }

    [Test]
    public void ManeuverabilityDataContainer_NoErrors()
    {
        var code = """
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataElements.DataElements;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.DataContainers;

public partial record ManeuverabilityDataContainer : DataContainerBase
{
    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Knots")]
    public decimal ManeuverabilityMaxSpeed { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Knots", NameLocalizationKey = "MaxReverseSpeed")]
    public decimal ManeuverabilityMaxReverseSpeed { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxSpeed", UnitKey = "Knots")]
    public decimal ManeuverabilitySubsMaxSpeedOnSurface { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxSpeed", UnitKey = "Knots")]
    public decimal ManeuverabilitySubsMaxSpeedAtPeriscope { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxSpeed", UnitKey = "Knots")]
    public decimal ManeuverabilitySubsMaxSpeedAtMaxDepth { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxReverseSpeed", UnitKey = "Knots", NameLocalizationKey = "ManeuverabilitySubsMaxSpeedOnSurface")]
    public decimal ManeuverabilitySubsMaxReverseSpeedOnSurface { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxReverseSpeed", UnitKey = "Knots", NameLocalizationKey = "ManeuverabilitySubsMaxSpeedAtPeriscope")]
    public decimal ManeuverabilitySubsMaxReverseSpeedAtPeriscope { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxReverseSpeed", UnitKey = "Knots", NameLocalizationKey = "ManeuverabilitySubsMaxSpeedAtMaxDepth")]
    public decimal ManeuverabilitySubsMaxReverseSpeedAtMaxDepth { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "MPS")]
    public decimal ManeuverabilitySubsMaxDiveSpeed { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal ManeuverabilitySubsDivingPlaneShiftTime { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal ManeuverabilityRudderShiftTime { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "M")]
    public decimal ManeuverabilityTurningCircle { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxSpeedTime", UnitKey = "S")]
    public decimal ForwardMaxSpeedTime { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxSpeedTime", UnitKey = "S")]
    public decimal ReverseMaxSpeedTime { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = "BlastProtection", TooltipKey = "BlastExplanation")]
    [DataElementFiltering(false)]
    public decimal RudderBlastProtection { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = "BlastProtection", TooltipKey = "BlastExplanation")]
    [DataElementFiltering(false)]
    public decimal EngineBlastProtection { get; set; }
}
""";
        var expected = """
using System;
using System.Collections.Generic;
using WoWsShipBuilder.DataElements.DataElements;

namespace WoWsShipBuilder.DataContainers;

#nullable enable
public partial record ManeuverabilityDataContainer
{
    private void UpdateDataElements()
    {
        DataElements.Clear();
    if (DataContainerBase.ShouldAdd(ManeuverabilityMaxSpeed))
        DataElements.Add(new KeyValueUnitDataElement("ShipStats_ManeuverabilityMaxSpeed", ManeuverabilityMaxSpeed.ToString(), "Unit_Knots"));

    if (DataContainerBase.ShouldAdd(ManeuverabilityMaxReverseSpeed))
        DataElements.Add(new KeyValueUnitDataElement("ShipStats_MaxReverseSpeed", ManeuverabilityMaxReverseSpeed.ToString(), "Unit_Knots"));

        var MaxSpeedList = new List<IDataElement>();
    if (DataContainerBase.ShouldAdd(ManeuverabilitySubsMaxSpeedOnSurface))
        MaxSpeedList.Add(new KeyValueUnitDataElement("ShipStats_ManeuverabilitySubsMaxSpeedOnSurface", ManeuverabilitySubsMaxSpeedOnSurface.ToString(), "Unit_Knots"));
    if (DataContainerBase.ShouldAdd(ManeuverabilitySubsMaxSpeedAtPeriscope))
        MaxSpeedList.Add(new KeyValueUnitDataElement("ShipStats_ManeuverabilitySubsMaxSpeedAtPeriscope", ManeuverabilitySubsMaxSpeedAtPeriscope.ToString(), "Unit_Knots"));
    if (DataContainerBase.ShouldAdd(ManeuverabilitySubsMaxSpeedAtMaxDepth))
        MaxSpeedList.Add(new KeyValueUnitDataElement("ShipStats_ManeuverabilitySubsMaxSpeedAtMaxDepth", ManeuverabilitySubsMaxSpeedAtMaxDepth.ToString(), "Unit_Knots"));
    if (MaxSpeedList.Count > 0)
        DataElements.Add(new GroupedDataElement("ShipStats_MaxSpeed", MaxSpeedList));

        var MaxReverseSpeedList = new List<IDataElement>();
    if (DataContainerBase.ShouldAdd(ManeuverabilitySubsMaxReverseSpeedOnSurface))
        MaxReverseSpeedList.Add(new KeyValueUnitDataElement("ShipStats_ManeuverabilitySubsMaxSpeedOnSurface", ManeuverabilitySubsMaxReverseSpeedOnSurface.ToString(), "Unit_Knots"));
    if (DataContainerBase.ShouldAdd(ManeuverabilitySubsMaxReverseSpeedAtPeriscope))
        MaxReverseSpeedList.Add(new KeyValueUnitDataElement("ShipStats_ManeuverabilitySubsMaxSpeedAtPeriscope", ManeuverabilitySubsMaxReverseSpeedAtPeriscope.ToString(), "Unit_Knots"));
    if (DataContainerBase.ShouldAdd(ManeuverabilitySubsMaxReverseSpeedAtMaxDepth))
        MaxReverseSpeedList.Add(new KeyValueUnitDataElement("ShipStats_ManeuverabilitySubsMaxSpeedAtMaxDepth", ManeuverabilitySubsMaxReverseSpeedAtMaxDepth.ToString(), "Unit_Knots"));
    if (MaxReverseSpeedList.Count > 0)
        DataElements.Add(new GroupedDataElement("ShipStats_MaxReverseSpeed", MaxReverseSpeedList));

    if (DataContainerBase.ShouldAdd(ManeuverabilitySubsMaxDiveSpeed))
        DataElements.Add(new KeyValueUnitDataElement("ShipStats_ManeuverabilitySubsMaxDiveSpeed", ManeuverabilitySubsMaxDiveSpeed.ToString(), "Unit_MPS"));

    if (DataContainerBase.ShouldAdd(ManeuverabilitySubsDivingPlaneShiftTime))
        DataElements.Add(new KeyValueUnitDataElement("ShipStats_ManeuverabilitySubsDivingPlaneShiftTime", ManeuverabilitySubsDivingPlaneShiftTime.ToString(), "Unit_S"));

    if (DataContainerBase.ShouldAdd(ManeuverabilityRudderShiftTime))
        DataElements.Add(new KeyValueUnitDataElement("ShipStats_ManeuverabilityRudderShiftTime", ManeuverabilityRudderShiftTime.ToString(), "Unit_S"));

    if (DataContainerBase.ShouldAdd(ManeuverabilityTurningCircle))
        DataElements.Add(new KeyValueUnitDataElement("ShipStats_ManeuverabilityTurningCircle", ManeuverabilityTurningCircle.ToString(), "Unit_M"));

        var MaxSpeedTimeList = new List<IDataElement>();
    if (DataContainerBase.ShouldAdd(ForwardMaxSpeedTime))
        MaxSpeedTimeList.Add(new KeyValueUnitDataElement("ShipStats_ForwardMaxSpeedTime", ForwardMaxSpeedTime.ToString(), "Unit_S"));
    if (DataContainerBase.ShouldAdd(ReverseMaxSpeedTime))
        MaxSpeedTimeList.Add(new KeyValueUnitDataElement("ShipStats_ReverseMaxSpeedTime", ReverseMaxSpeedTime.ToString(), "Unit_S"));
    if (MaxSpeedTimeList.Count > 0)
        DataElements.Add(new GroupedDataElement("ShipStats_MaxSpeedTime", MaxSpeedTimeList));

        var BlastProtectionList = new List<IDataElement>();

        BlastProtectionList.Add(new TooltipDataElement("ShipStats_RudderBlastProtection", RudderBlastProtection.ToString(), "ShipStats_BlastExplanation", ""));

        BlastProtectionList.Add(new TooltipDataElement("ShipStats_EngineBlastProtection", EngineBlastProtection.ToString(), "ShipStats_BlastExplanation", ""));
    if (BlastProtectionList.Count > 0)
        DataElements.Add(new GroupedDataElement("ShipStats_BlastProtection", BlastProtectionList));
    }
}
#nullable restore
""";

        _ = VerifyGenerator(code, expected);
    }

    [Test]
    public void SingleKeyValueUnitElement_NoErrors()
    {
        var code = """
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataElements.DataElements;

namespace WoWsShipBuilder.DataContainers;

public partial record TestContainer : DataContainerBase
{
    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Knots")]
    public decimal TestProperty { get; set; }
}
""";
        var expected = """
using System;
using System.Collections.Generic;
using WoWsShipBuilder.DataElements.DataElements;

namespace WoWsShipBuilder.DataContainers;

#nullable enable
public partial record TestContainer
{
    private void UpdateDataElements()
    {
        DataElements.Clear();
        if (DataContainerBase.ShouldAdd(TestProperty))
            DataElements.Add(new KeyValueUnitDataElement("ShipStats_TestProperty", TestProperty.ToString(), "Unit_Knots"));
    }
}
#nullable restore
""";

        _ = VerifyGenerator(code, expected);
    }

    [Test]
    public void GroupedKeyValueUnitElement_NoErrors()
    {
        var code = """
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataElements.DataElements;

namespace WoWsShipBuilder.DataContainers;

public partial record TestContainer : DataContainerBase
{
    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "TestGroup", UnitKey = "Knots")]
    public decimal TestProperty { get; set; }
}
""";
        var expected = """
using System;
using System.Collections.Generic;
using WoWsShipBuilder.DataElements.DataElements;

namespace WoWsShipBuilder.DataContainers;

#nullable enable
public partial record TestContainer
{
    private void UpdateDataElements()
    {
        DataElements.Clear();

        var TestGroupList = new List<IDataElement>();
        if (DataContainerBase.ShouldAdd(TestProperty))
            TestGroupList.Add(new KeyValueUnitDataElement("ShipStats_TestProperty", TestProperty.ToString(), "Unit_Knots"));
        if (TestGroupList.Count > 0)
            DataElements.Add(new GroupedDataElement("ShipStats_TestGroup", TestGroupList));


    }
}
#nullable restore
""";

        _ = VerifyGenerator(code, expected);
    }

    private static GeneratorDriverRunResult VerifyGenerator(string source, string generated = "")
    {
        var baseInput = """
using WoWsShipBuilder.DataElements.DataElements;

namespace WoWsShipBuilder.Data.Generator.Test.TestStructures;

public record ProjectileDataContainer : DataContainerBase;
""";

        var compilation = CreateCompilation(baseInput, source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new DataElementSourceGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out ImmutableArray<Diagnostic> _);

        var result = driver.GetRunResult();
        result.Diagnostics.Should().BeEmpty();

        if (!string.IsNullOrEmpty(generated))
        {
            var expectedTree = CSharpSyntaxTree.ParseText(generated);
            result.GeneratedTrees.Should().Contain(syntaxTree => syntaxTree.IsEquivalentTo(expectedTree, false));
        }

        return result;
    }

    private static Compilation CreateCompilation(params string[] source)
    {
        List<SyntaxTree> syntaxTrees = source.Select(sourceFile => CSharpSyntaxTree.ParseText(sourceFile)).ToList();

        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        return CSharpCompilation.Create("compilation",
            syntaxTrees,
            new[]
            {
                MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(typeof(DataElementTypeAttribute).GetTypeInfo().Assembly.Location),
            },
            new(OutputKind.ConsoleApplication));
    }

}
