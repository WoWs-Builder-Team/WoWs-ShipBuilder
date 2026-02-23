using FluentAssertions;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure.GameData;

namespace WoWsShipBuilder.Test.Infrastructure.GameData.ShipModuleTests
{
    [TestFixture]
    public class SortShipUpgrades
    {
        [Test]
        public void SortShipUpgrades_SimpleUnsortedList()
        {
            var data = this.CreateUpgradeList();

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
            return new()
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
