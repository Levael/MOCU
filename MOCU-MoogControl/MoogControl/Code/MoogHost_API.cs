using System;
using System.Collections.Generic;

using MoogModule.Daemon;


namespace MoogModule
{
    public interface MoogHost_API  // Is a mirror of 'MoogDaemon_API'
    {
        // commands from Host
        event Action<MachineSettings> Connect;
        event Action Engage;
        event Action Disengage;
        event Action Reset;
        event Action StartReceivingFeedback;
        event Action StopReceivingFeedback;
        event Action<MoveToPointParameters> MoveToPoint;
        event Action<MoveByTrajectoryParameters> MoveByTrajectory;

        event Action<object?> Test;

        // responses from Daemon
        void State(MoogRealTimeState state);
        void Feedback(MoogFeedback feedback);
    }
}