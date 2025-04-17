using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MoogModule
{
    public interface MoogDaemon_API
    {
        void Connect(ConnectParameters parameters);
        void Engage();
        void Disengage();
        void Reset();
        void StartReceivingFeedback();
        void StopReceivingFeedback();
        void MoveToPoint(MoveToPointParameters parameters);
        void MoveByTrajectory(MoveByTrajectoryParameters parameters);

        event Action<DofParameters> SingleFeedback;
        event Action<IEnumerable<DofParameters>> FeedbackForTimeRange;
    }
}