using System;
using System.Collections.Generic;
using System.Diagnostics;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public static class BallisticHelper
    {
        // Physical Constants                               Description                  | Units
        private const double G = 9.8;                    // Gravitational Constant       | m/(s^2)
        private const double T0 = 288.15;                // Temperature at Sea Level     | K
        private const double L = 0.0065;                 // Atmospheric Lapse Rate       | C/m
        private const double P0 = 101325;                // Pressure at Sea Level        | Pa
        private const double R = 8.31447;                // Ideal Gas Constant           | J/(mol K)
        private const double M = 0.0289644;              // Molarity of Air at Sea Level | kg/mol
        private const double TimeMultiplier = 2.75;      // In game time multiplier

        // Calculation Parameters
        private static double maxAngles = 600;           // Max Angle                    | degrees
        private static double angleStep = 0.00174533;    // Angle Step                   | degrees    60 * Math.PI / 180. / n_angle //ELEV. ANGLES 0-30 deg, at launch
        private static double dt = 0.02;                 // Time step                    | s
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

            for (int i = 0; i < maxAngles; i++)
            {
                list.Add(i * angleStep);
            }

            return list;
        }

        private static double GetNormalization(double caliber)
        {
            double norm = 0;
            if (caliber <= 0.139)
            {
                norm = 10 * Math.PI / 180;
            }
            else if (caliber <= 0.152)
            {
                norm = 8.5 * Math.PI / 180;
            }
            else if (caliber <= 0.24)
            {
                norm = 7 * Math.PI / 180;
              }
            else if (caliber < 0.51)
            {
                norm = 6 * Math.PI / 180;
            }
            else
            {
                norm = 15 * Math.PI / 180;     
            }

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

            // Increase max range toa ccount for modifiers
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
            var initialPen = CalculatePen(initialSpeed, shell.Caliber, shell.Mass, shell.Krupp);
            var initialBallistic = new Ballistic(initialPen, initialSpeed, 0, 0);
            dict.Add(0, initialBallistic);
            var lastRange = 0d;

            foreach (var angle in calculationAngles)
            {
                // Various variable inizialitazion
                var y = 0d;
                var x = 0d;
                var t = 0d;
                var v_x = initialSpeed * Math.Cos(angle);
                var v_y = initialSpeed * Math.Sin(angle);

                while (y >= 0)
                {
                    x += dt * v_x;
                    y += dt * v_y;
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var T = T0 - (L * y);
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                    var p = P0 * Math.Pow(T / T0, G * M / R / L);

                    var rho_g = (p * M) / R / T;

                    var speed = Math.Sqrt((v_x * v_x) + (v_y * v_y));

                    v_x = v_x - (dt * k * rho_g * v_x * speed);

                    v_y = v_y - (dt * G) - (dt * k * rho_g * v_y * speed);

                    t += dt;
                }

                var v_impact = Math.Sqrt((v_x * v_x) + (v_y * v_y));
                var impactAngle = Math.Atan2(Math.Abs(v_y), Math.Abs(v_x)) * (180 / Math.PI);
                var pen = CalculatePen(v_impact, shell.Caliber, shell.Mass, shell.Krupp);

                if (x > maxRange || x < lastRange)
                {
                    break;
                }

                var ballistic = new Ballistic(pen, v_impact, t / TimeMultiplier, impactAngle);
                dict.Add(x, ballistic);
                lastRange = x;
            }

            return dict;
        }
    }

    public record Ballistic
    {
        public Ballistic(double penetration, double velocity, double flightTime, double impactAngle)
        {
            Penetration = penetration;
            Velocity = velocity;
            FlightTime = flightTime;
            ImpactAngle = impactAngle;
        }

        public double Penetration { get; set; }

        public double Velocity { get; set; }

        public double FlightTime { get; set; }

        public double ImpactAngle { get; set; }
    }
}
