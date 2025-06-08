﻿using System;
using System.Numerics;


namespace MoogModule.Daemon
{
    public class MoogRealTimeState
    {
        private (DofParameters coordinate, DateTime time) _currentPosition;
        private (DofParameters coordinate, DateTime time) _previousPosition;
        private (DofParameters coordinate, DateTime time) _prePreviousPosition;


        public EncodedMachineState EncodedMachineState { get; set; } = EncodedMachineState.Disabled;
        public DofParameters DesiredPosition { get; set; } = default;
        public string Faults { get; set; } = String.Empty;


        public DofParameters Position
        {
            get => _currentPosition.coordinate;

            set
            {
                _prePreviousPosition = _previousPosition;
                _previousPosition = _currentPosition;
                _currentPosition = (coordinate: value, time: DateTime.UtcNow);
            }
        }
        
        public DofParameters Velocity
        {
            get
            {
                var lastMoveDisplacement = _currentPosition.coordinate - _previousPosition.coordinate;
                var lastMoveDuration = (float)(_currentPosition.time - _previousPosition.time).TotalSeconds;

                if (lastMoveDuration < 1e-6f)   // too small
                    return DofParameters.NaN;

                return lastMoveDisplacement / lastMoveDuration;
            }
        }

        public DofParameters Acceleration
        {
            get
            {
                var currentDisplacement = _currentPosition.coordinate - _previousPosition.coordinate;
                var previousDisplacement = _previousPosition.coordinate - _prePreviousPosition.coordinate;

                var lastMoveDuration = (float)(_currentPosition.time - _previousPosition.time).TotalSeconds;
                var previousMoveDuration = (float)(_previousPosition.time - _prePreviousPosition.time).TotalSeconds;

                if (lastMoveDuration < 1e-6f || previousMoveDuration < 1e-6f)   // too small
                    return DofParameters.NaN;

                var currentVelocity = currentDisplacement / lastMoveDuration;
                var previousVelocity = previousDisplacement / previousMoveDuration;

                return (currentVelocity - previousVelocity) / lastMoveDuration;
            }
        }
    }
}