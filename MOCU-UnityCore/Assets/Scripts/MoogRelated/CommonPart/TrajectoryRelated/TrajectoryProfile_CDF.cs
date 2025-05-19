using System;


namespace MoogModule
{
    public static class TrajectoryProfile_CDF
    {
        private const float SqrtTwo = 1.414213562f;
        private const double ExpMinusHalf = 0.6065306597; // Approximation of e^(-0.5)

        /// <param name="x">Normalized time in range [0, 1]</param>
        /// <param name="sigmas">Number of sigmas within half the bell curve</param>
        /// <returns>Normalized displacement in range [0, 1]</returns>
        public static float NormalizedDisplacement(float x, float sigmas)
        {
            if (x < 0f || x > 1f)
                throw new ArgumentOutOfRangeException(nameof(x), "Time must be in range [0, 1].");

            if (x == 0f) return 0f;
            if (x == 1f) return 1f;

            float mean = 0.5f;
            float sigma = 1.0f / (2 * sigmas);

            return 0.5f * (1.0f + Erf((x - mean) / (sigma * SqrtTwo)));
        }

        public static float InverseNormalizedDisplacement(float y, float sigmas)
        {
            if (y < 0f || y > 1f)
                throw new ArgumentOutOfRangeException(nameof(y), "Displacement must be in range [0, 1].");

            if (y == 0f) return 0f;
            if (y == 1f) return 1f;

            float sigma = 1.0f / (2f * sigmas);

            return 0.5f + ErfInv(2f * y - 1f) * sigma * SqrtTwo;
        }

        /// <param name="distance">Total physical distance [meters]</param>
        /// <param name="duration">Total duration [seconds]</param>
        /// <param name="sigmas">Number of sigmas within half the bell curve</param>
        /// <returns>Maximum acceleration in meters per second squared</returns>
        public static double MaxAcceleration(double distance, double duration, double sigmas)
        {
            double scale = (2 * sigmas) * ExpMinusHalf;
            return distance / (duration * duration) * scale;
        }

        /// <summary>
        /// Approximation of the error function (erf) using numerical constants.
        /// Based on Abramowitz and Stegun formula 7.1.26.
        /// </summary>
        /// <param name="z">Input value</param>
        /// <returns>Approximate value of erf(x)</returns>
        private static float Erf(float z)
        {
            float sign = z < 0 ? -1 : 1;
            z = Math.Abs(z);

            float a1 = 0.254829592f;
            float a2 = -0.284496736f;
            float a3 = 1.421413741f;
            float a4 = -1.453152027f;
            float a5 = 1.061405429f;
            float p = 0.3275911f;

            float t = 1.0f / (1.0f + p * z);
            float y = 1.0f - (((((a5 * t + a4) * t + a3) * t + a2) * t + a1) * t) * (float)Math.Exp(-z * z);

            return sign * y;
        }

        /// <summary>
        /// Approximation of the inverse error function erf⁻¹(z), valid for z ∈ (-1, 1).
        /// Uses a fast Winitzki approximation for |z| ≤ 0.8, and a more accurate rational approximation near the edges.
        /// This hybrid approach ensures both speed and accuracy across the full domain.
        /// </summary>
        /// <param name="z">Value in range (-1, 1), where typically z = 2y - 1 for normalized CDF</param>
        /// <returns>Approximate value of erf⁻¹(z)</returns>
        private static float ErfInv(float z)
        {
            if (z <= -1f || z >= 1f)
                throw new ArgumentOutOfRangeException(nameof(z), "z must be in (-1, 1)");

            float absZ = Math.Abs(z);
            float result;

            if (absZ <= 0.8f)
            {
                // Winitzki approximation (fast and accurate for central values)
                float a = 0.147f;
                float ln = (float)Math.Log(1f - z * z);
                float first = (2f / (MathF.PI * a)) + (ln / 2f);
                float second = ln / a;
                result = Math.Sign(z) * (float)Math.Sqrt(Math.Sqrt(first * first - second) - first);
            }
            else
            {
                // Improved rational approximation (accurate for |z| > 0.8)
                float ln = (float)Math.Log(1f - absZ * absZ);
                float t = 1f / (float)Math.Sqrt(-ln);
                float p = (((1.641345311f * t + 3.429567803f) * t - 1.62490649f) * t - 1.970840454f) /
                          ((1.637067800f * t + 3.543889200f) * t + 1f);
                result = z < 0f ? -p : p;
            }

            return result;
        }
    }
}