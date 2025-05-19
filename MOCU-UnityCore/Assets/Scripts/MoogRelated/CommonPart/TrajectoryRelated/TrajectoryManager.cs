using MoogModule.Daemon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;


namespace MoogModule
{
    public class TrajectoryManager
    {
        private MoveByTrajectoryParameters _trajectoryParameters;
        private MoogRealTimeState _moogRealTimeState;
        private MachineSettings _machineSettings;

        private ITrajectoryGenerator _trajectoryGenerator;
        private DofParameters[] _trajectory;
        private int _currentPositionIndex;
        

        public TrajectoryManager(MoveByTrajectoryParameters trajectoryParameters, MachineSettings machineSettings, MoogRealTimeState moogRealTimeState)
        {
            _trajectoryParameters = trajectoryParameters;
            _moogRealTimeState = moogRealTimeState;
            _machineSettings = machineSettings;

            _trajectoryGenerator = trajectoryParameters.TrajectoryType switch
            {
                TrajectoryType.Linear => new LinearTrajectoryGenerator(_trajectoryParameters),
                _ => throw new NotImplementedException()
            };

            _trajectory = _trajectoryGenerator.GetWholePath(_machineSettings.DesiredFPS);
            _currentPositionIndex = 0;
        }

        public DofParameters? GetNextPosition()
        {
            if (_currentPositionIndex == _trajectory.Length)
                return null;

            int indexesLeft = _trajectory.Length - _currentPositionIndex - 1;
            int totalIndexes = _trajectory.Length;
            TimeSpan totalTime = _trajectoryParameters.MovementDuration;
            TimeSpan timePassed = DateTime.UtcNow - _trajectoryParameters.ScheduledStartTime;
            TimeSpan timeLeft = totalTime - timePassed;

            return _trajectoryParameters.DelayHandling switch
            {
                DelayCompensationStrategy.None => throw new Exception("You didn't choose a 'DelayCompensationStrategy'"),
                DelayCompensationStrategy.Ignore => _trajectory[_currentPositionIndex++],
                DelayCompensationStrategy.Repair => _trajectory[_currentPositionIndex += (int)(indexesLeft / timeLeft.TotalMilliseconds)],
                DelayCompensationStrategy.Jump => _trajectory[(int)Math.Round(totalIndexes * timePassed / totalTime)],
                _ => throw new NotImplementedException()
            };
        }

        public bool TooEarly()
        {
            return (DateTime.UtcNow - _trajectoryParameters.ScheduledStartTime).TotalSeconds < 0;
        }

        public bool TooLate()
        {
            return (DateTime.UtcNow - _trajectoryParameters.ScheduledStartTime) > _trajectoryParameters.MovementDuration;
        }
    }
}