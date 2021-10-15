namespace WoWsShipBuilder.Core.DataUI
{
    public record ManeuverabilityUI
    {
        public decimal ManeuverabilityMaxSpeed { get; set; }

        public decimal ManeuverabilityFullPowerForward { get; set; }

        public decimal ManeuverabilityFullPowerBackward { get; set; }

        public decimal ManeuverabilityPowerToWeight { get; set; }

        public decimal ManeuverabilityTurningCircle { get; set; }

        public decimal ManeuverabilityRudderShiftTime { get; set; }
    }
}
