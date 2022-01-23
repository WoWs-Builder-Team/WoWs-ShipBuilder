using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record CamouflageUI : IDataUi
    {
        [JsonIgnore]
        public string Name { get; set; } = default!;

        [JsonIgnore]
        public string Type { get; set; } = default!;

        public Dictionary<string, float> Modifiers { get; set; } = null!;
        
        //public static CamouflageUI GetCamouflage(string shipIndex)
        //{
        //    Exterior exterior = new ();

        //    if (!(AppData.ext?.TryGetValue(consumableIdentifier, out consumable!) ?? false))
        //    {
        //        Logging.Logger.Error("Camouflage {} not found in cached consumable list. Using dummy consumable instead.", consumableIdentifier);

        //    }

        //}
    }
}
