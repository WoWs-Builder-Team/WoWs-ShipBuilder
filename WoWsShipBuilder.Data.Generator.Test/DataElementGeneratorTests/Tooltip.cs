﻿namespace WoWsShipBuilder.Data.Generator.Test.DataElementGeneratorTests;

public partial class DataElementGeneratorTest
{
    [Test]
    [Category("KeyValueUnit")]
    public async Task GenerateCode_Tooltip_Success()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial class TestDataContainer : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Tooltip, TooltipKey = "TestTooltip")]
                         public decimal Prop1 { get; set; }
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
                                   if (global::WoWsShipBuilder.DataElements.DataContainerBase.ShouldAdd(this.Prop1))
                                   {
                                       this.DataElements.Add(new global::WoWsShipBuilder.DataElements.TooltipDataElement("ShipStats_Prop1", this.Prop1.ToString(), "ShipStats_TestTooltip", ""));
                                   }
                               }
                           }
                       }

                       """;

        await CreateTest(source, expected).RunAsync();
    }

    [Test]
    [Category("KeyValueUnit")]
    public async Task GenerateCode_TooltipWithUnit_Success()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial class TestDataContainer : DataContainerBase
                     {
                         [DataElementType(DataElementTypes.Tooltip, TooltipKey = "TestTooltip", UnitKey="Knots")]
                         public decimal Prop1 { get; set; }
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
                                   if (global::WoWsShipBuilder.DataElements.DataContainerBase.ShouldAdd(this.Prop1))
                                   {
                                       this.DataElements.Add(new global::WoWsShipBuilder.DataElements.TooltipDataElement("ShipStats_Prop1", this.Prop1.ToString(), "ShipStats_TestTooltip", "Unit_Knots"));
                                   }
                               }
                           }
                       }

                       """;

        await CreateTest(source, expected).RunAsync();
    }
}
