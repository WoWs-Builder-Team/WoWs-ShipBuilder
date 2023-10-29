namespace WoWsShipBuilder.Data.Generator.Test.DataElementAnalyzerTests;

public partial class DataElementAnalyzerTest
{
    [Test]
    public async Task AnalyzeTooltip_AllRequiredParams_NoDiagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Tooltip, TooltipKey = "Test")]
                         public decimal Prop1 { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeTooltip_TooltipKeyMissing_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Tooltip)]
                         public decimal {|SB1002:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeTooltip_UnitKeySpecified_NoDiagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Tooltip, TooltipKey = "Test", UnitKey = "Test")]
                         public decimal Prop1 { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeTooltip_LocalizationKeySpecified_NoDiagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Tooltip, TooltipKey = "Test", LocalizationKeyOverride = "Test")]
                         public decimal Prop1 { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeTooltip_CollectionNameSpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Tooltip, TooltipKey = "Test", ArgumentsCollectionName = "Values")]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeTooltip_TreatArgumentsAsLocalizationSpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Tooltip, TooltipKey = "Test", ArgumentsTextKind = TextKind.LocalizationKey)]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeTooltip_TreatArgumentsAsAppLocalizationSpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Tooltip, TooltipKey = "Test", ArgumentsTextKind = TextKind.AppLocalizationKey)]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeTooltip_TreatValueAsLocalizationKeySpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Tooltip, TooltipKey = "Test", ValueTextKind = TextKind.LocalizationKey)]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeTooltip_TreatValueAsAppLocalizationKeySpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Tooltip, TooltipKey = "Test", ValueTextKind = TextKind.AppLocalizationKey)]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }
}
