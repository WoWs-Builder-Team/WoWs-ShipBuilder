using System;
using System.Collections.Generic;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataContainers
{
    public static class BallisticHelper
    {
        // Physical Constants                                                       Description                  | Units
        private const double G = 9.8;                                            // Gravitational Constant       | m/(s^2)
        private const double T0 = 288.15;                                        // Temperature at Sea Level     | K
        private const double L = 0.0065;                                         // Atmospheric Lapse Rate       | C/m
        private const double P0 = 101325;                                        // Pressure at Sea Level        | Pa
        private const double R = 8.31447;                                        // Ideal Gas Constant           | J/(mol K)
        private const double M = 0.0289644;                                      // Molarity of Air at Sea Level | kg/mol

        // Calculation Parameters
        private const double MaxAngles = 600;                                    // Max Angle                    | degrees
        private const double AngleStep = 0.00174533;                             // Angle Step                   | degrees    60 * Math.PI / 180. / n_angle //ELEV. ANGLES 0-30 deg, at launch
        private const double Dt = 0.02;                                          // Time step                    | s
        private static List<double> calculationAngles = new();

        /// <summary>
        /// Calculate the shell penetration given the parameter.
        /// </summary>
        /// <param name="velocity">Velocity of the shell.</param>
        /// <param name="diameter">Diameter of the shell.</param>
        /// <param name="mass">Mass of the shell.</param>
        /// <param name="krupp">Krupp of the shell.</param>
        /// <returns>The penetration value.</returns>
        private static double CalculatePen(double velocity, double diameter, double mass, double krupp)
        {
            // Raw Penetration(mm) = 0.00046905491615181766 * V(m / s) ^ 1.4822064892953855 * D(m) ^ -0.6521 * M(kg) ^ 0.5506 * K / 2400
            return 0.00046905491615181766 * Math.Pow(velocity, 1.4822064892953855) * Math.Pow(diameter, -0.6521) * Math.Pow(mass, 0.5506) * (krupp / 2400);
        }

        /// <summary>
        /// Create the angles for the ballistic calculations.
        /// </summary>
        /// <returns>A list of the angle used to calculate the ballistic statistic.</returns>
        private static List<double> CreateCalculationAngles()
        {
            var list = new List<double>();

            for (int i = 0; i < MaxAngles; i++)
            {
                list.Add(i * AngleStep);
            }

            return list;
        }

        private static double GetNormalization(double caliber)
        {
            double norm = caliber switch
            {
                <= 0.139 => 10 * Math.PI / 180,
                <= 0.152 => 8.5 * Math.PI / 180,
                <= 0.24 => 7 * Math.PI / 180,
                < 0.51 => 6 * Math.PI / 180,
                _ => 15 * Math.PI / 180,
            };

            return norm;
        }

        /// <summary>
        /// Calculate all <see cref="Ballistic"/> values for a shell.
        /// </summary>
        /// <param name="shell">The shell to calculate the <see cref="Ballistic"/> of.</param>
        /// <param name="maxRange">The max range of the ship.</param>
        /// <returns>A dictionary with the <see cref="Ballistic"/> for each range.</returns>
        public static Dictionary<double, Ballistic> CalculateBallistic(ArtilleryShell shell, double maxRange)
        {
            var dict = new Dictionary<double, Ballistic>();

            // Increase max range to account for modifiers
            maxRange *= 1.5;

            // Initialize the angle list. This way we calculate it only once, if it's needed.
            //  No reason to calculate them if the user never try to see the ballistic data.
            if (calculationAngles.Count == 0)
            {
                calculationAngles = CreateCalculationAngles();
            }

            var k = 0.5 * shell.AirDrag * Math.Pow(shell.Caliber / 2, 2) * Math.PI / shell.Mass;

            // Insert pen at 0 distance
            var initialSpeed = shell.MuzzleVelocity;
            var initialPen = shell.ShellType == ShellType.AP ? CalculatePen(initialSpeed, shell.Caliber, shell.Mass, shell.Krupp) : shell.Penetration;
            var initialBallistic = new Ballistic(initialPen, initialSpeed, 0, 0, new());
            dict.Add(0, initialBallistic);
            var lastRange = 0d;

            foreach (double angle in calculationAngles)
            {
                var coordinates = new List<Coordinates>
                {
                    new(0, 0),
                };

                // Various variable initialization
                var y = 0d;
                var x = 0d;
                var t = 0d;
                var vX = initialSpeed * Math.Cos(angle);
                var vY = initialSpeed * Math.Sin(angle);

                while (y >= 0)
                {
                    x += Dt * vX;
                    y += Dt * vY;
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var T = T0 - (L * y);
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                    var p = P0 * Math.Pow(T / T0, G * M / R / L);

                    var rhoG = (p * M) / R / T;

                    var speed = Math.Sqrt((vX * vX) + (vY * vY));

                    vX -= Dt * k * rhoG * vX * speed;

                    vY = vY - (Dt * G) - (Dt * k * rhoG * vY * speed);

                    t += Dt;

                    if (y >= 0)
                    {
                        coordinates.Add(new(x, y));
                    }
                }

                var vImpact = Math.Sqrt((vX * vX) + (vY * vY));
                var impactAngle = Math.Atan2(Math.Abs(vY), Math.Abs(vX)) * (180 / Math.PI);
                var pen = shell.ShellType == ShellType.AP ? CalculatePen(vImpact, shell.Caliber, shell.Mass, shell.Krupp) : shell.Penetration;

                if (x > maxRange || x < lastRange)
                {
                    break;
                }

                var ballistic = new Ballistic(pen, vImpact, t / (double)Constants.TimeScale, impactAngle, coordinates);
                dict.Add(x, ballistic);
                lastRange = x;
            }

            return dict;
        }
    }

    public sealed record Ballistic(double Penetration, double Velocity, double FlightTime, double ImpactAngle, List<Coordinates> Coordinates);

    public sealed record Coordinates(double X, double Y);
}
