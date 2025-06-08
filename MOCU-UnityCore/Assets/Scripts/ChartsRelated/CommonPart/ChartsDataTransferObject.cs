using System;
using System.Collections.Generic;
using System.Linq;

using DaemonsRelated;


namespace ChartsModule
{
    public class ChartsDataTransferObject : IDataTransferObject
    {
        // Commands
        /*public bool ConnectCommand { get; set; } = false;
        public bool EngageCommand { get; set; } = false;
        public bool DisengageCommand { get; set; } = false;
        public bool ResetCommand { get; set; } = false;
        public bool DoReceiveFeedback { get; set; } = false;
        public bool MoveToPointCommand { get; set; } = false;
        public bool MoveByTrajectoryCommand { get; set; } = false;*/

        // Parameters
        /*public MachineSettings? ConnectParameters { get; set; } = null;
        public MoveToPointParameters? MoveToPointParameters { get; set; } = null;
        public MoveByTrajectoryParameters? MoveByTrajectoryParameters { get; set; } = null;*/

        // Info
        /*public MoogRealTimeState? State { get; set; } = null;
        public MoogFeedback? Feedback { get; set; } = null;*/

        // Common for every DTO
        public IEnumerable<DaemonErrorReport> DaemonErrorReports { get; set; } = Enumerable.Empty<DaemonErrorReport>();
        public bool DoTerminateTheDaemon { get; set; } = false;
        public string CustomMessage { get; set; } = String.Empty;
    }
}