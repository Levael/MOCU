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
        private ITrajectoryProfile _trajectoryProfile;
        private DofParameters[] _trajectory;

        private int _currentPositionIndex;
        private DateTime _startTime;
        

        public TrajectoryManager(MoveByTrajectoryParameters trajectoryParameters, MachineSettings machineSettings, MoogRealTimeState moogRealTimeState)
        {
            _trajectoryParameters = trajectoryParameters;
            _moogRealTimeState = moogRealTimeState;
            _machineSettings = machineSettings;

            _trajectoryProfile = trajectoryParameters.TrajectoryProfile switch
            {
                TrajectoryProfile.CDF => new TrajectoryProfile_CDF(trajectoryParameters.TrajectoryProfileSettings.CDF),
                _ => throw new NotImplementedException()
            };

            _trajectoryGenerator = trajectoryParameters.TrajectoryType switch
            {
                TrajectoryType.Linear => new TrajectoryGenerator_Linear(_trajectoryParameters, _trajectoryProfile),
                _ => throw new NotImplementedException()
            };

            _trajectory = _trajectoryGenerator.GetWholePath(_machineSettings.DesiredFPS);   // todo: return 0   <----------------------------
            _currentPositionIndex = 0;
            _startTime = _trajectoryParameters.ScheduledStartTime == DateTime.MinValue ? DateTime.UtcNow : _trajectoryParameters.ScheduledStartTime;


            Console.WriteLine(@$"
                TrajectoryManager.constructor info:
                    _trajectory.Length = {_trajectory.Length}
                    _currentPositionIndex = {_currentPositionIndex}
                    _startTime = {_startTime}
            ");
        }

        public DofParameters? GetNextPosition()
        {
            if (_currentPositionIndex == _trajectory.Length)
                return null;

            int indexesLeft = _trajectory.Length - _currentPositionIndex - 1;
            int totalIndexes = _trajectory.Length;
            TimeSpan totalTime = _trajectoryParameters.MovementDuration;
            TimeSpan timePassed = DateTime.UtcNow - _startTime;
            TimeSpan timeLeft = totalTime - timePassed;

            Console.WriteLine(@$"
                TrajectoryManager.GetNextPosition info:
                    indexesLeft = {indexesLeft}
                    totalIndexes = {totalIndexes}
                    totalTime = {totalTime}
                    timePassed = {timePassed}
                    timeLeft = {timeLeft}
            ");

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