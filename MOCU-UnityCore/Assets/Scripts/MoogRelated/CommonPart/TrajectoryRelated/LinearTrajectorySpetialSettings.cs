using System;


namespace MoogModule
{
    public class LinearTrajectorySpetialSettings
    {
        public Func<float, float> NormalizedProgressFunction { get; set; } = t => TrajectoryProfile_CDF.NormalizedDisplacement(t, sigmas: 3.0f);
    }
}