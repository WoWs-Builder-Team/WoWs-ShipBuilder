using System.Collections.Generic;
using Newtonsoft.Json;

namespace WoWsShipBuilder.Core.DataUI.Projectiles
{
    public record ProjectileUI : IDataUi
    {
        [JsonIgnore]
        public List<KeyValuePair<string, string>>? ProjectileData { get; set; }
    }
}
