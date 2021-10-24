namespace WoWsShipBuilder.Core.DataUI
{
    public record TorpedoArmamentUi
    {
        public string Name { get; set; } = default!;

        public decimal TurnTime { get; set; }

        public decimal TraverseSpeed { get; set; }
    }
}
