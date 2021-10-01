using System.Collections.Generic;

namespace WoWsShipBuilder.Core.BuildCreator
{
    public class Build
    {
        public Build(string? shipName, List<string>? modules, List<string>? skills, List<string>? signals)
        {
            ShipName = shipName;
            Modules = modules;
            Skills = skills;
            Signals = signals;
        }

        public Build()
        {
        }

        public string? BuildName { get; set; }

        public string? ShipName { get; set; }

        public List<string>? Modules { get; set; }

        public List<string>? Skills { get; set; }

        public List<string>? Signals { get; set; }
    }
}
