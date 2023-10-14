﻿namespace WoWsShipBuilder.Data.Generator.Test.DataElementGeneratorTests;

public partial class DataElementGeneratorTest
{
    [Test]
    [Category("KeyValue")]
    public async Task GenerateCode_KeyValueNoLocalization_Success()
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

        var expected = """
                       // <auto-generated />
                       #nullable enable
                       namespace Test
                       {
                           public partial record TestRecord
                           {
                               private void UpdateDataElements()
                               {
                                   this.DataElements.Clear();
                                   if (global::WoWsShipBuilder.DataElements.DataContainerBase.ShouldAdd(this.Prop1))
                                   {
                                       this.DataElements.Add(new global::WoWsShipBuilder.DataElements.KeyValueDataElement("ShipStats_Prop1", this.Prop1.ToString(), global::WoWsShipBuilder.DataElements.DataElementTextKind.Plain));
                                   }
                               }
                           }
                       }

                       """;

        await CreateTest(source, expected).RunAsync();
    }

    [Test]
    [Category("KeyValue")]
    public async Task GenerateCode_KeyValueGameLocalization_Success()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.KeyValue, ValueTextKind = TextKind.LocalizationKey)]
                         public decimal Prop1 { get; set; }
                     }
                     """;

        var expected = """
                       // <auto-generated />
                       #nullable enable
                       namespace Test
                       {
                           public partial record TestRecord
                           {
                               private void UpdateDataElements()
                               {
                                   this.DataElements.Clear();
                                   if (global::WoWsShipBuilder.DataElements.DataContainerBase.ShouldAdd(this.Prop1))
                                   {
                                       this.DataElements.Add(new global::WoWsShipBuilder.DataElements.KeyValueDataElement("ShipStats_Prop1", this.Prop1.ToString(), global::WoWsShipBuilder.DataElements.DataElementTextKind.LocalizationKey));
                                   }
                               }
                           }
                       }

                       """;

        await CreateTest(source, expected).RunAsync();
    }

    [Test]
    [Category("KeyValue")]
    public async Task GenerateCode_KeyValueAppLocalization_Success()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.KeyValue, ValueTextKind = TextKind.AppLocalizationKey)]
                         public decimal Prop1 { get; set; }
                     }
                     """;

        var expected = """
                       // <auto-generated />
                       #nullable enable
                       namespace Test
                       {
                           public partial record TestRecord
                           {
                               private void UpdateDataElements()
                               {
                                   this.DataElements.Clear();
                                   if (global::WoWsShipBuilder.DataElements.DataContainerBase.ShouldAdd(this.Prop1))
                                   {
                                       this.DataElements.Add(new global::WoWsShipBuilder.DataElements.DataElements.KeyValueDataElement("ShipStats_Prop1", this.Prop1.ToString(), global::WoWsShipBuilder.DataElements.DataElements.DataElementTextKind.AppLocalizationKey));
                                   }
                               }
                           }
                       }

                       """;

        await CreateTest(source, expected).RunAsync();
    }
}
