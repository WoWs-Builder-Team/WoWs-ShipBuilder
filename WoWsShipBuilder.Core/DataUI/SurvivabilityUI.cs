// ReSharper disable InconsistentNaming
namespace WoWsShipBuilder.Core.DataUI
{
    public record SurvivabilityUI
    {
        public decimal SurvivabilityHitPoints { get; set; }

        public decimal SurvivabilityFireDuration { get; set; }

        public decimal SurvivabilityFireAmount { get; set; }

        public decimal SurvivabilityFireReduction { get; set; }

        public decimal SurvivabilityFireDPS { get; set; }

        public decimal SurvivabilityFireTotalDamage { get; set; }

        public decimal SurvivabilityFloodDuration { get; set; }

        public decimal SurvivabilityFloodAmount { get; set; }

        public decimal SurvivabilityFloodProbability { get; set; }

        public decimal SurvivabilityFloodTorpedoProtection { get; set; }

        public decimal SurvivabilityFloodDPS { get; set; }

        public decimal SurvivabilityFloodTotalDamage { get; set; }
    }
}
