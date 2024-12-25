namespace WoWsShipBuilder.Data.Generator.Test.DataElementAnalyzerTests;

public partial class DataElementAnalyzerTest
{
    [Test]
    public async Task AnalyzeFormattedText_AllRequiredParams_NoDiagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial class TestDataContainer : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.FormattedText, ArgumentsCollectionName="Test")]
                         public decimal Prop1 { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeFormattedText_ArgumentCollectionNameMissing_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial class TestDataContainer : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.FormattedText)]
                         public decimal {|SB1002:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeFormattedText_TreatArgumentsAsLocalizationSpecified_NoDiagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial class TestDataContainer : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.FormattedText, ArgumentsCollectionName = "Test", ArgumentsTextKind = TextKind.LocalizationKey)]
                         public decimal Prop1 { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeFormattedText_TreatArgumentsAsAppLocalizationSpecified_NoDiagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial class TestDataContainer : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.FormattedText, ArgumentsCollectionName = "Test", ArgumentsTextKind = TextKind.AppLocalizationKey)]
                         public decimal Prop1 { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeFormattedText_LocalizationKeyOverrideSpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial class TestDataContainer : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.FormattedText, ArgumentsCollectionName = "Test", LocalizationKeyOverride = "Test")]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeFormattedText_UnitKeySpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial class TestDataContainer : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.FormattedText, ArgumentsCollectionName = "Test", UnitKey = "Test")]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeFormattedText_TooltipKeySpecified_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial class TestDataContainer : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.FormattedText, ArgumentsCollectionName = "Test", TooltipKey = "Test")]
                         public decimal {|SB1003:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }
}
