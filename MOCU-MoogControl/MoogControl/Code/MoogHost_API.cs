using System;
using System.Collections.Generic;

using DaemonsRelated;


namespace MoogModule
{
    public interface MoogHost_API  // Is a mirror of 'MoogDaemon_API'
    {
        // commands from Host
        event Action Engage;
        event Action Disengage;
        event Action Reset;
        event Action StartReceivingFeedback;
        event Action StopReceivingFeedback;
        event Action<MoveToPointParameters> MoveToPoint;
        event Action<MoveByTrajectoryParameters> MoveByTrajectory;

        // responses from Daemon
        void SingleFeedback(DofParameters parameters);
        void FeedbackForTimeRange(IEnumerable<DofParameters> parameters);
    }
}