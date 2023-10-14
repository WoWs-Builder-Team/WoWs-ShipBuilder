namespace WoWsShipBuilder.Data.Generator.Test.DataElementAnalyzerTests;

public partial class DataElementAnalyzerTest
{
    [Test]
    public async Task AnalyzeKeyValue_AllRequiredParams_NoDiagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.KeyValue)]
                         public decimal Prop1 { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeKeyValue_LocalizationKeyOverrideSpecified_NoDiagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.KeyValue, LocalizationKeyOverride = "Test")]
                         public decimal Prop1 { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeKeyValue_CollectionNameSpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.KeyValue, ArgumentsCollectionName = "Values")]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeKeyValue_TreatArgumentsAsLocalizationSpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.KeyValue, ArgumentsTextKind = TextKind.LocalizationKey)]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeKeyValue_TreatArgumentsAsAppLocalizationSpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.KeyValue, ArgumentsTextKind = TextKind.AppLocalizationKey)]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeKeyValue_UnitKeySpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.KeyValue, UnitKey="Test")]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeKeyValue_TooltipKeySpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.KeyValue, TooltipKey="Test")]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }
}
