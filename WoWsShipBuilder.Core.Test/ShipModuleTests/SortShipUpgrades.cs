using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.Test.ShipModuleTests
{
    [TestFixture]
    public class SortShipUpgrades
    {
        [Test]
        public void SortShipUpgrades_SimpleUnsortedList()
        {
            var data = CreateUpgradeList();

            var sortedData = ShipModuleHelper.GroupAndSortUpgrades(data);

            foreach ((ComponentType type, var upgrades) in sortedData)
            {
                var upgradeTypes = upgrades.Select(x => x.UcType).Distinct().ToList();
                upgradeTypes.Single().Should().Be(type);

                upgrades[0].Prev.Should().BeNullOrEmpty();
                upgrades[1].Prev.Should().Be(upgrades[0].Name);
                upgrades[2].Prev.Should().Be(upgrades[1].Name);
            }
        }

        private List<ShipUpgrade> CreateUpgradeList()
        {
            return new List<ShipUpgrade>
            {
                new() { UcType = ComponentType.Artillery, Name = "3", Prev = "2" },
                new() { UcType = ComponentType.Artillery, Name = "1", Prev = "" },
                new() { UcType = ComponentType.Artillery, Name = "2", Prev = "1" },
                new() { UcType = ComponentType.Hull, Name = "2", Prev = "1" },
                new() { UcType = ComponentType.Hull, Name = "3", Prev = "2" },
                new() { UcType = ComponentType.Hull, Name = "1", Prev = "" },
            };
        }
    }
}
