﻿namespace WoWsShipBuilder.Data.Generator.Test.DataElementGeneratorTests;

public partial class DataElementGeneratorTest
{
    [Test]
    [Category("Grouped")]
    public async Task GenerateCode_OneGroupTwoElements_Success()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial class TestDataContainer : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "Loaders")]
                         public string BowLoaders { get; set; } = default!;

                         [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "Loaders")]
                         public string SternLoaders { get; set; } = default!;
                     }
                     """;

        var expected = """
                       // <auto-generated />
                       #nullable enable
                       namespace Test
                       {
                           public partial class TestDataContainer
                           {
                               private void UpdateDataElements()
                               {
                                   this.DataElements.Clear();
                                   var LoadersList = new global::System.Collections.Generic.List<global::WoWsShipBuilder.DataElements.IDataElement>();
                                   if (global::WoWsShipBuilder.DataElements.DataContainerBase.ShouldAdd(this.BowLoaders))
                                   {
                                       LoadersList.Add(new global::WoWsShipBuilder.DataElements.KeyValueDataElement("ShipStats_BowLoaders", this.BowLoaders, global::WoWsShipBuilder.DataElements.DataElementTextKind.Plain));
                                   }

                                   if (global::WoWsShipBuilder.DataElements.DataContainerBase.ShouldAdd(this.SternLoaders))
                                   {
                                       LoadersList.Add(new global::WoWsShipBuilder.DataElements.KeyValueDataElement("ShipStats_SternLoaders", this.SternLoaders, global::WoWsShipBuilder.DataElements.DataElementTextKind.Plain));
                                   }

                                   if (LoadersList.Count > 0)
                                   {
                                       this.DataElements.Add(new global::WoWsShipBuilder.DataElements.GroupedDataElement("ShipStats_Loaders", LoadersList));
                                   }
                               }
                           }
                       }

                       """;

        await CreateTest(source, expected).RunAsync();
    }

    [Test]
    [Category("Grouped")]
    public async Task GenerateCode_TwoGroupsMixedOrder_Success()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial class TestDataContainer : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "Group1")]
                         public string Prop1 { get; set; } = default!;

                         [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "Group2")]
                         public string Prop2 { get; set; } = default!;

                         [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "Group1")]
                         public string Prop3 { get; set; } = default!;

                         [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "Group2")]
                         public string Prop4 { get; set; } = default!;
                     }
                     """;

        var expected = """
                       // <auto-generated />
                       #nullable enable
                       namespace Test
                       {
                           public partial class TestDataContainer
                           {
                               private void UpdateDataElements()
                               {
                                   this.DataElements.Clear();
                                   var Group1List = new global::System.Collections.Generic.List<global::WoWsShipBuilder.DataElements.IDataElement>();
                                   if (global::WoWsShipBuilder.DataElements.DataContainerBase.ShouldAdd(this.Prop1))
                                   {
                                       Group1List.Add(new global::WoWsShipBuilder.DataElements.KeyValueDataElement("ShipStats_Prop1", this.Prop1, global::WoWsShipBuilder.DataElements.DataElementTextKind.Plain));
                                   }

                                   if (global::WoWsShipBuilder.DataElements.DataContainerBase.ShouldAdd(this.Prop3))
                                   {
                                       Group1List.Add(new global::WoWsShipBuilder.DataElements.KeyValueDataElement("ShipStats_Prop3", this.Prop3, global::WoWsShipBuilder.DataElements.DataElementTextKind.Plain));
                                   }

                                   if (Group1List.Count > 0)
                                   {
                                       this.DataElements.Add(new global::WoWsShipBuilder.DataElements.GroupedDataElement("ShipStats_Group1", Group1List));
                                   }

                                   var Group2List = new global::System.Collections.Generic.List<global::WoWsShipBuilder.DataElements.IDataElement>();
                                   if (global::WoWsShipBuilder.DataElements.DataContainerBase.ShouldAdd(this.Prop2))
                                   {
                                       Group2List.Add(new global::WoWsShipBuilder.DataElements.KeyValueDataElement("ShipStats_Prop2", this.Prop2, global::WoWsShipBuilder.DataElements.DataElementTextKind.Plain));
                                   }

                                   if (global::WoWsShipBuilder.DataElements.DataContainerBase.ShouldAdd(this.Prop4))
                                   {
                                       Group2List.Add(new global::WoWsShipBuilder.DataElements.KeyValueDataElement("ShipStats_Prop4", this.Prop4, global::WoWsShipBuilder.DataElements.DataElementTextKind.Plain));
                                   }

                                   if (Group2List.Count > 0)
                                   {
                                       this.DataElements.Add(new global::WoWsShipBuilder.DataElements.GroupedDataElement("ShipStats_Group2", Group2List));
                                   }
                               }
                           }
                       }

                       """;

        await CreateTest(source, expected).RunAsync();
    }
}
