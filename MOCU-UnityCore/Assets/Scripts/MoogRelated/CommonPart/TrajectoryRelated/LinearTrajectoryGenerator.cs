using System;
using System.Collections.Generic;


namespace MoogModule
{
    public class LinearTrajectoryGenerator : ITrajectoryGenerator
    {
        private MoveByTrajectoryParameters _parameters;
        private LinearTrajectorySpetialSettings _spetialSettings;
        private Func<float, float> NormalizedProgressFunction;

        public LinearTrajectoryGenerator(MoveByTrajectoryParameters parameters)
        {
            _parameters = parameters;
            _spetialSettings = parameters.TrajectorySpetialSettings as LinearTrajectorySpetialSettings;
            NormalizedProgressFunction = _spetialSettings.NormalizedProgressFunction;
        }

        public DofParameters[] GetWholePath(int fps)
        {
            int totalPoints = (int)(_parameters.MovementDuration.TotalSeconds * fps);
            DofParameters[] trajectory = new DofParameters[totalPoints];
            float deltaTime = 1.0f / fps;

            for (int i = 0; i < totalPoints; i++)
            {
                float currentTime = i * deltaTime;
                float normalizedTime = currentTime / (float)_parameters.MovementDuration.TotalSeconds;
                float progress = NormalizedProgressFunction(normalizedTime);
                trajectory[i] = Interpolate(_parameters.StartPoint, _parameters.EndPoint, progress);
            }

            return trajectory;
        }

        // 'Jump' scenario
        public DofParameters? GetNextPosition()
        {
            TimeSpan timePassed = DateTime.UtcNow - _parameters.ScheduledStartTime;
            float normalizedTime = (float)(timePassed / _parameters.MovementDuration);

            if (normalizedTime > 1 || normalizedTime < 0)
                return null;

            float normalizedDisplacement = NormalizedProgressFunction(normalizedTime);
            return Interpolate(_parameters.StartPoint, _parameters.EndPoint, normalizedDisplacement);
        }

        private DofParameters Interpolate(DofParameters start, DofParameters end, float progress)
        {
            return start + (end - start) * progress;
        }
    } 
}