namespace WoWsShipBuilder.Data.Generator.Test.DataElementAnalyzerTests;

public partial class DataElementAnalyzerTest
{
    [Test]
    public async Task AnalyzeValue_AllRequiredParams_NoDiagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Value)]
                         public decimal Prop1 { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeValue_CollectionNameSpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Value, ArgumentsCollectionName = "Values")]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeValue_TreatArgumentsAsLocalizationSpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Value, ArgumentsTextKind = TextKind.LocalizationKey)]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeValue_TreatArgumentsAsAppLocalizationSpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Value, ArgumentsTextKind = TextKind.AppLocalizationKey)]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeValue_UnitKeySpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Value, UnitKey = "Test")]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeValue_TooltipKeySpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Value, TooltipKey = "Test")]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeValue_LocalizationKeySpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Value, LocalizationKeyOverride = "Test")]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }
}
