using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record CarrierPlaneUI
    {
        // Plane Stats
        public string? CarrierPlaneName { get; set; }

        public string? CarrierPlaneType { get; set; }

        public string? CarrierPlaneOnDeck { get; set; }

        public string? CarrierPlaneInFlight { get; set; }

        public string? CarrierPlaneInAttackingFlight { get; set; }

        public decimal? CarrierPlaneAircraftHP { get; set; }

        public decimal? CarrierPlaneSpeed { get; set; }

        public decimal? CarrierPlaneRecoveryTime { get; set; }

        // General payload stats
        public decimal? CarrierPlanePayloadName { get; set; }

        public decimal? CarrierPlanePayloadType { get; set; }

        public decimal? CarrierPlanePayloadDamage { get; set; }

        public decimal? CarrierPlanePayloadPerPlane { get; set; }

        public decimal? CarrierPlaneStrikeTime { get; set; }

        // Torpedo Bombers
        public decimal? CarrierPlaneTorpedoRange { get; set; }

        public decimal? CarrierPlaneTorpedoArmDistance { get; set; }

        public decimal? CarrierPlaneTorpedoFloodingChance { get; set; }

        // (Skip)Bombers
        public decimal? CarrierPlaneBombPenetration { get; set; }

        public decimal? CarrierPlaneBombSkips { get; set; }

        public decimal? CarrierPlaneFireChance { get; set; }

        public decimal? CarrierPlaneDepthExplosion { get; set; }

        // Maybe for AP Bombers, not sure these stats exist
        public decimal? CarrierPlaneArmingThreshold { get; set; }

        public decimal? CarrierPlaneFuseTimer { get; set; }

        // Rockets
        public decimal? CarrierPlaneRocketDelay { get; set; }
    }
}
