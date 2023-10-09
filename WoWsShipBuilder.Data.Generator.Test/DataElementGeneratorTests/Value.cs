﻿namespace WoWsShipBuilder.Data.Generator.Test.DataElementGeneratorTests;

public partial class DataElementGeneratorTest
{
    [Test]
    [Category("Value")]
    public async Task GenerateCode_ValueNoLocalization_Success()
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
                                   if (global::WoWsShipBuilder.DataElements.DataElements.DataContainerBase.ShouldAdd(this.Prop1))
                                   {
                                       this.DataElements.Add(new global::WoWsShipBuilder.DataElements.DataElements.ValueDataElement(this.Prop1.ToString(), global::WoWsShipBuilder.DataElements.DataElements.DataElementTextKind.Plain));
                                   }
                               }
                           }
                       }

                       """;

        await CreateTest(source, expected).RunAsync();
    }

    [Test]
    [Category("Value")]
    public async Task GenerateCode_ValueGameLocalization_Success()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Value, ValueTextKind = TextKind.LocalizationKey)]
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
                                   if (global::WoWsShipBuilder.DataElements.DataElements.DataContainerBase.ShouldAdd(this.Prop1))
                                   {
                                       this.DataElements.Add(new global::WoWsShipBuilder.DataElements.DataElements.ValueDataElement(this.Prop1.ToString(), global::WoWsShipBuilder.DataElements.DataElements.DataElementTextKind.LocalizationKey));
                                   }
                               }
                           }
                       }

                       """;

        await CreateTest(source, expected).RunAsync();
    }

    [Test]
    [Category("Value")]
    public async Task GenerateCode_ValueAppLocalization_Success()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Value, ValueTextKind = TextKind.AppLocalizationKey)]
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
                                   if (global::WoWsShipBuilder.DataElements.DataElements.DataContainerBase.ShouldAdd(this.Prop1))
                                   {
                                       this.DataElements.Add(new global::WoWsShipBuilder.DataElements.DataElements.ValueDataElement(this.Prop1.ToString(), global::WoWsShipBuilder.DataElements.DataElements.DataElementTextKind.AppLocalizationKey));
                                   }
                               }
                           }
                       }

                       """;

        await CreateTest(source, expected).RunAsync();
    }
}
