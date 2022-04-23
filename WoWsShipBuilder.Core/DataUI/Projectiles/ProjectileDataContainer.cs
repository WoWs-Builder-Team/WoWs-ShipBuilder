using System.Collections.Generic;

namespace WoWsShipBuilder.Core.DataUI
{
    public record ProjectileDataContainer : DataContainerBase
    {
        public List<KeyValuePair<string, string>>? ProjectileData { get; set; }
    }
}
