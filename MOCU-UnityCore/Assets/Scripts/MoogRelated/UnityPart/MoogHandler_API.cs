using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MoogModule
{
    public interface MoogHandler_API
    {
        void Connect(MachineSettings parameters);
        void Engage();
        void Disengage();
        void Reset();
        void MoveToPoint(MoveToPointParameters parameters);
        void MoveByTrajectory(MoveByTrajectoryParameters parameters);
        void GetFeedbackForTimeRange(TimeSpan start, TimeSpan end);
    }
}