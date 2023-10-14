namespace WoWsShipBuilder.Data.Generator.Test.DataElementAnalyzerTests;

public partial class DataElementAnalyzerTest
{
    [Test]
    public async Task AnalyzeKeyValueUnit_AllRequiredParams_NoDiagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Test")]
                         public decimal Prop1 { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeKeyValueUnit_LocalizationKeyOverrideSpecified_NoDiagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Test", LocalizationKeyOverride = "Test")]
                         public decimal Prop1 { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeKeyValueUnit_UnitKeyMissing_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.KeyValueUnit)]
                         public decimal {|SB1002:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeKeyValueUnit_CollectionNameSpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Test", ArgumentsCollectionName = "Values")]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeKeyValueUnit_TreatArgumentsAsLocalizationSpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Test", ArgumentsTextKind = TextKind.LocalizationKey)]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeKeyValueUnit_TreatArgumentsAsAppLocalizationSpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Test", ArgumentsTextKind = TextKind.LocalizationKey)]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeKeyValueUnit_TooltipKeySpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Test", TooltipKey = "Test")]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeKeyValueUnit_TreatValueAsLocalizationKeySpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Test", ValueTextKind = TextKind.LocalizationKey)]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeKeyValueUnit_TreatValueAsAppLocalizationKeySpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Test", ValueTextKind = TextKind.AppLocalizationKey)]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }
}
