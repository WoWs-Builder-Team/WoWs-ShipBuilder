namespace WoWsShipBuilder.Core.DataUI
{
    public record TorpedoUI
    {
        public string TorpedoName { get; set; } = default!;

        public decimal TorpedoRange { get; set; }

        public decimal TorpedoDamage { get; set; }

        public decimal TorpedoReload { get; set; }

        public decimal TorpedoSpeed { get; set; }

        public decimal TorpedoDetectability { get; set; }

        public decimal TorpedoReactionTime { get; set; }

        public decimal TorpedoFloodingChance { get; set; }
    }
}
