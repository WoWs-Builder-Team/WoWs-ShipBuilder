namespace WoWsShipBuilder.Data.Generator.Test.DataElementAnalyzerTests;

public partial class DataElementAnalyzerTest
{
    [Test]
    public async Task AnalyzeGrouped_SecondaryTypeSpecified_NoDiagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord: DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "Loaders")]
                         public string Prop1 { get; set; } = default!;
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeGrouped_SecondaryTypeMissing_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord: DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Grouped, GroupKey = "Loaders")]
                         public string {|SB0002:Prop1|} { get; set; } = default!;
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeGrouped_GroupKeyMissing_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord: DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue)]
                         public string {|SB1001:Prop1|} { get; set; } = default!;
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task AnalyzeGrouped_NoGroupGroupKeyExists_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord: DataContainerBase
                     {
                         [DataElementType(DataElementTypes.KeyValue, GroupKey="Test")]
                         public string {|SB1003:Prop1|} { get; set; } = default!;
                     }
                     """;

        await CreateTest(source).RunAsync();
    }
}
