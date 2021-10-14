namespace WoWsShipBuilder.Core.DataProvider
{
    record ShipUI
    {
        // Main battery
        public string? MainBatteryName { get; set; }

        public decimal? MainBatteryRange { get; set; }

        public decimal? MainBatteryReload { get; set; }

        public decimal? MainBatteryRoF { get; set; }

        public decimal? MainBatteryTurnTime { get; set; }

        public decimal? MainBatteryTraverseSpeed { get; set; }

        public decimal? MainBatterySigma { get; set; }

        public decimal? MainBatteryHorizontalDisp { get; set; }

        public decimal? MainBatteryVerticalDisp { get; set; }

        // Shell
        // not sure if needed because its a itemscontrol
        public string? ShellName { get; set; }

        public string? ShellType { get; set; }

        public decimal? ShellDamage { get; set; }

        public decimal? ShellTheoreticalDPM { get; set; }

        public decimal? ShellVelocity { get; set; }

        public decimal? ShellWeight { get; set; }

        public decimal? ShellPenetration { get; set; }

        public decimal? ShellFireChance { get; set; }

        public decimal? ShellRicochet { get; set; }

        public decimal? ShellOvermatch { get; set; }

        public decimal? ShellArmingThreshold { get; set; }

        public decimal? ShellFuseTimer { get; set; }

        public decimal? ShellDepthExplosion { get; set; }

        // Torpedoes
        public string? TorpedoName { get; set; }

        public decimal? TorpedoRange { get; set; }

        public decimal? TorpedoDamage { get; set; }

        public decimal? TorpedoReload { get; set; }

        public decimal? TorpedoSpeed { get; set; }

        public decimal? TorpedoDetectability { get; set; }

        public decimal? TorpedoReactionTime { get; set; }

        public decimal? TorpedoFloodingChance { get; set; }

        public decimal? TorpedoTurnTime { get; set; }

        public decimal? TorpedoTraverseSpeed { get; set; }

        // Airstrike maybe seperate between ASW/Dutch and CV planes
        public string? AirstrikeName { get; set; }

        public string? AirstrikeFlights { get; set; }

        public decimal? AirstrikeReload { get; set; }

        public decimal? AirstrikeAircraftHP { get; set; }

        public decimal? AirstrikePayload { get; set; }

        public decimal? AirstrikeBombType { get; set; }

        public decimal? AirstrikeTorpedoRange { get; set; }

        public decimal? AirstrikeTorpedoArmDistance { get; set; }

        public decimal? AirstrikeTorpedoFloodingChance { get; set; }

        public decimal? AirstrikeDamage { get; set; }

        public decimal? AirstrikePenetration { get; set; }

        public decimal? AirstrikeFireChance { get; set; }

        public decimal? AirstrikeRange { get; set; }

        public decimal? AirstrikeDelay { get; set; }

        public decimal? AirstrikeSpeed { get; set; }

        public decimal? AirstrikeDepthExplosion { get; set; }

        // Secondary not sure if needed since it should be itemscontrol
        public string? SecondaryName { get; set; }

        public decimal? SecondaryRange { get; set; }

        public decimal? SecondaryReload { get; set; }

        public decimal? SecondaryRoF { get; set; }

        public decimal? SecondaryHorizontalDisp { get; set; }

        public decimal? SecondaryVerticalDisp { get; set; }

        public decimal? SecondaryDamage { get; set; }

        public decimal? SecondaryPenetration { get; set; }

        public decimal? SecondaryFireChance { get; set; }

        // Maneuvrability
        public decimal ManeuvrabilitySpeed { get; set; }

        public decimal ManeuvrabilityFullPowerForward { get; set; }

        public decimal ManeuvrabilityFullPowerBackward { get; set; }

        public decimal ManeuvrabilityPowerToWeight { get; set; }

        public decimal ManeuvrabilityTurningCircle { get; set; }

        public decimal ManeuvrabilityRudderShiftTime { get; set; }

        // Concealment
        public decimal ConcealmentBySea { get; set; }

        public decimal ConcealmentBySeaFiring { get; set; }

        public decimal ConcealmentBySeaFiringSmoke { get; set; }

        public decimal ConcealmentBySeaFiringAA { get; set; }

        public decimal ConcealmentBySeaFire { get; set; }

        public decimal ConcealmentByAir { get; set; }

        public decimal ConcealmentByAirFiring { get; set; }

        public decimal ConcealmentByAirFiringAA { get; set; }

        public decimal ConcealmentByAirFire { get; set; }

        public decimal ConcealmentBySub { get; set; }

        public decimal ConcealmentBySubSurface { get; set; }

        public decimal ConcealmentBySubPeriscope { get; set; }

        public decimal ConcealmentBySubOperating { get; set; }

        public decimal ConcealmentBySubMaximum { get; set; }

        // Survivability
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
