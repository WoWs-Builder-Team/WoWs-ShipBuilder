using System;

namespace WoWsShipBuilder.Core.DataUI
{
    public static class MathHelper
    {
        /// <summary>
        /// Create a sample from a gaussian distribution.
        /// </summary>
        /// <param name="random">A <seealso cref="Random"/> instance.</param>
        /// <param name="mean">The mean of the gaussian function.</param>
        /// <param name="standardDeviation">The standard deviation of the gaussian function.</param>
        /// <returns>A sample from a gaussian distribution.</returns>
        public static double GaussianSample(Random random, double mean, double standardDeviation)
        {
            var x = random.NextDouble();
            var y = random.NextDouble();
            var normalStd = Math.Sqrt(-2.0 * Math.Log(x)) * Math.Sin(2.0 * Math.PI * y);
            var normal = mean + (standardDeviation * normalStd);
            return normal;
        }

        /// <summary>
        /// Creates a sample from a gaussian distribution within a given interval. If the sample falls outside the interval it is changed with another one from a uniform distribution within the interval.
        /// </summary>
        /// <param name="random">A <seealso cref="Random"/> instance.</param>
        /// <param name="mean">The mean of the gaussian function.</param>
        /// <param name="standardDeviation">The standard deviation of the gaussian function.</param>
        /// <param name="min">Min of the interval.</param>
        /// <param name="max">Max of the interval.</param>
        /// <returns>An adjusted sample from a gaussian distribution within a given interval.</returns>
        public static double AdjustedGaussian(Random random, double mean, double standardDeviation, double min, double max)
        {
            var rng = GaussianSample(random, mean, standardDeviation);
            if (rng < min || rng > max)
            {
                rng = (random.NextDouble() * (max - min)) + min;
            }

            return rng;
        }

        /// <summary>
        /// Calculates the Stand-alone error function ERF of a variable.
        /// </summary>
        /// <param name="x">The variable of the ERF function.</param>
        /// <returns>The ERF of the given variable.</returns>
        public static double Erf(double x)
        {
            const double a1 = 0.254829592;
            const double a2 = -0.284496736;
            const double a3 = 1.421413741;
            const double a4 = -1.453152027;
            const double a5 = 1.061405429;
            const double p = 0.3275911;

            int sign = 1;
            if (x < 0)
            {
                sign = -1;
            }

            x = Math.Abs(x);

            double t = 1.0 / (1.0 + (p * x));
            double y = 1.0 - (((((((((a5 * t) + a4) * t) + a3) * t) + a2) * t) + a1) * t * Math.Exp(-x * x));

            return sign * y;
        }

        /// <summary>
        /// Calculates the  cumulative density function (CDF) of a standard normal (Gaussian) random variable.
        /// </summary>
        /// <param name="x">The variable of the CDF function.</param>
        /// <returns>The CDF of the given variable.</returns>
        public static double Cdf(double x)
        {
            return (1 + Erf(x / Math.Sqrt(2))) / 2;
        }

        /// <summary>
        /// Calculates the inverse of the <see cref="Erf(double)"/> function.
        /// </summary>
        /// <param name="x">The variable of the ERFinv function.</param>
        /// <returns>The inverse ERF function of the given variable.</returns>
        public static double MbgErfInv(double x)
        {
            double p;
            double w = -Math.Log((1.01 - x) * (1.01 + x));
            if (w < 5.0000001)
            {
                w = w - 2.5000001;
                p = 2.81022636e-081;
                p = 3.43273939e-071 + (p * w);
                p = -3.5233877e-061 + (p * w);
                p = -4.39150654e-061 + (p * w);
                p = 0.000218580871 + (p * w);
                p = -0.001253725031 + (p * w);
                p = -0.004177681641 + (p * w);
                p = 0.2466407271 + (p * w);
                p = 1.501409411 + (p * w);
            }
            else
            {
                w = Math.Sqrt(w) - 3.0000001;
                p = -0.0002002142571;
                p = 0.0001009505581 + (p * w);
                p = 0.001349343221 + (p * w);
                p = -0.003673428441 + (p * w);
                p = 0.005739507731 + (p * w);
                p = -0.00762246131 + (p * w);
                p = 0.009438870471 + (p * w);
                p = 1.001674061 + (p * w);
                p = 2.832976821 + (p * w);
            }

            return p * x;
        }

        /// <summary>
        /// Calculates the inverse of the <see cref="Cdf(double)"/> function.
        /// </summary>
        /// <param name="x">The variable of the InvCDF function.</param>
        /// <returns>The inverse CDF function of the given variable.</returns>
        public static double InvCdf(double x)
        {
            return Math.Sqrt(2) * MbgErfInv((2 * x) - 1);
        }
    }
}
