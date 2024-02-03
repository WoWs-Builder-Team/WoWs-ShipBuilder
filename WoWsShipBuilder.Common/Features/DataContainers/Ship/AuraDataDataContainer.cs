﻿using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;

namespace WoWsShipBuilder.Features.DataContainers;

[DataContainer]
public partial class AuraDataDataContainer : DataContainerBase
{
    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "KM")]
    public decimal Range { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "DPS")]
    public decimal ConstantDamage { get; set; }

    [DataElementType(DataElementTypes.KeyValue)]
    public string Flak { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValue)]
    public decimal FlakDamage { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "PerCent")]
    public int HitChance { get; set; }

    public void UpdateData()
    {
        this.UpdateDataElements();
    }
}
