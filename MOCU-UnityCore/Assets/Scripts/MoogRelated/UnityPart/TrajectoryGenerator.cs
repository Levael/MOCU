using System;
using System.Collections.Generic;


namespace MoogModule
{
    public class LinearTrajectorySettings
    {
        public DofParameters StartPoint { get; set; }
        public DofParameters EndPoint { get; set; }
        public float TotalTimeSeconds { get; set; } = 1.0f;
        public int FramesPerSecond { get; set; } = 1000;
        public Func<float, float> NormalizedProgressFunction { get; set; } = t => TrajectoryProfile.CDF(t, 3.0f);
    }

    public static class TrajectoryProfile
    {
        public static float CDF(float normalizedTime, float numberOfSigmas)
        {
            // normalizedTime ∈ [0, 1]
            float mean = 0.5f;
            float sigma = 1.0f / (2 * numberOfSigmas);
            return 0.5f * (1.0f + Erf((normalizedTime - mean) / (sigma * (float)Math.Sqrt(2.0))));
        }

        public static float SmoothStep(float normalizedTime)
        {
            // smoother than linear, but not as smooth as others
            return normalizedTime * normalizedTime * (3 - 2 * normalizedTime);
        }

        public static float SmootherStep(float normalizedTime)
        {
            // even smoother start/stop
            return normalizedTime * normalizedTime * normalizedTime * (normalizedTime * (6 * normalizedTime - 15) + 10);
        }

        public static float Linear(float normalizedTime)
        {
            return normalizedTime;
        }

        private static float Erf(float x)
        {
            float sign = x < 0 ? -1 : 1;
            x = Math.Abs(x);

            float a1 = 0.254829592f;
            float a2 = -0.284496736f;
            float a3 = 1.421413741f;
            float a4 = -1.453152027f;
            float a5 = 1.061405429f;
            float p = 0.3275911f;

            float t = 1.0f / (1.0f + p * x);
            float y = 1.0f - (((((a5 * t + a4) * t + a3) * t + a2) * t + a1) * t) * (float)Math.Exp(-x * x);

            return sign * y;
        }
    }

    public static class TrajectoryGenerator
    {
        public static DofParameters[] Generate(LinearTrajectorySettings settings)
        {
            var totalPoints = (int)(settings.TotalTimeSeconds * settings.FramesPerSecond);
            var trajectory = new DofParameters[totalPoints];
            float deltaTime = 1.0f / settings.FramesPerSecond;

            for (int i = 0; i < totalPoints; i++)
            {
                float currentTime = i * deltaTime;
                float normalizedTime = currentTime / settings.TotalTimeSeconds;
                float progress = settings.NormalizedProgressFunction(normalizedTime);
                trajectory[i] = InterpolateLinear(settings.StartPoint, settings.EndPoint, progress);
            }

            return trajectory;
        }

        private static DofParameters InterpolateLinear(DofParameters start, DofParameters end, float progress)
        {
            return new DofParameters
            {
                Roll = LinearInterpolation(start.Roll, end.Roll, progress),
                Pitch = LinearInterpolation(start.Pitch, end.Pitch, progress),
                Yaw = LinearInterpolation(start.Yaw, end.Yaw, progress),
                Surge = LinearInterpolation(start.Surge, end.Surge, progress),
                Sway = LinearInterpolation(start.Sway, end.Sway, progress),
                Heave = LinearInterpolation(start.Heave, end.Heave, progress)
            };
        }

        private static float LinearInterpolation(float from, float to, float t)
        {
            return from + (to - from) * t;
        }
    }

    // todo: refactor and move to other place
    public static class TrajectoryMath
    {
        public static double CalculateMaxAcceleration(double distanceMeters, double durationSeconds, double sigmaCount = 3)
        {
            const double expMinusHalf = 0.6065306597; // e^(-0.5)
            double scale = (2 * sigmaCount) * expMinusHalf;
            return distanceMeters / (durationSeconds * durationSeconds) * scale;
        }
    }
}