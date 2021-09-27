using System.Collections.Generic;

namespace WoWsShipBuilder.Core.BuildCreator
{
    public class Build
    {
        public string? ShipName { get; set; }

        public List<string>? Modules { get; set; }

        public List<string>? Skills { get; set; }

        public List<string>? Signals { get; set; }
    }
}
