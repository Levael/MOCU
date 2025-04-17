using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace MoogModule
{
    public class MoogHandler : ManagedMonoBehaviour, MoogHandler_API
    {
        public ModuleStatusHandler<Moog_ModuleSubStatuses> stateTracker { get; private set; }

        private MoogHostSideBridge _daemon;
        private ConnectParameters _connectParameters;

        // unity components
        private DaemonsHandler _daemonsHandler;
        private TrajectoryMakerForMoogTest _controller;

        // ########################################################################################

        public override void ManagedAwake()
        {
            _daemonsHandler = GetComponent<DaemonsHandler>();
            _controller = GetComponent<TrajectoryMakerForMoogTest>();

            _connectParameters = new ConnectParameters
            {
                StartPosition = new DofParameters { Heave = -0.22f },
                MaxAcceleration = 0.5,
                HOST_IP = "192.168.2.3",
                HOST_PORT = "16386",
                MBC_IP = "192.168.2.1",
                MBC_PORT = "16384"
            };
        }

        public override void ManagedStart()
        {
            var daemonWrapper = _daemonsHandler.GetDaemon(DaemonType.Moog);
            var communicator = daemonWrapper.Communicator;

            _daemon = new MoogHostSideBridge(communicator);
            _daemon.Connect(_connectParameters);    // will be sent only after 'ConnectionEstablished' (automatically, it's in the queue)

            //communicator.ConnectionEstablished += message => stateTracker.UpdateSubStatus(Moog_ModuleSubStatuses.Communicator, SubStatusState.Complete);
            //communicator.ConnectionBroked += message => stateTracker.UpdateSubStatus(Moog_ModuleSubStatuses.Communicator, SubStatusState.Failed);
            _daemon.MachineStateChanged += OnMachineStateChanged;
            _controller.OnPositionChanged += (coordinate) => MoveToPoint(new MoveToPointParameters { Coordinate = coordinate });    //test

            // here communicator and process start
            daemonWrapper.Start();
        }

        // ########################################################################################

        public void Connect(ConnectParameters parameters)
        {
        }

        public void Engage()
        {
        }

        public void Disengage()
        {
        }

        public void Reset()
        {
        }

        public void MoveToPoint(MoveToPointParameters parameters)
        {
            /*if (stateTracker.Status == ModuleStatus.FullyOperational)
                _daemon.MoveToPoint(parameters);*/
        }

        public void MoveByTrajectory(MoveByTrajectoryParameters parameters)
        {
        }

        public void GetFeedbackForTimeRange(TimeSpan start, TimeSpan end)
        {

        }

        // ########################################################################################

        private void OnMachineStateChanged(MachineState state)
        {
            /*if (state == MachineState.Ready || state == MachineState.Engaged)
                stateTracker.UpdateSubStatus(Moog_ModuleSubStatuses.Machine, SubStatusState.Complete);
            else if (state == MachineState.Initializing || state == MachineState.Standby || state == MachineState.Parking)
                stateTracker.UpdateSubStatus(Moog_ModuleSubStatuses.Machine, SubStatusState.StillNotSet);
            else
                stateTracker.UpdateSubStatus(Moog_ModuleSubStatuses.Machine, SubStatusState.Failed);*/
        }
    }
}