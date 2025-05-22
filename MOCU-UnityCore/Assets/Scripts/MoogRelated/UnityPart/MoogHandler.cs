using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using MoogModule.Daemon;
using System.Collections;
using UnityEngine;


namespace MoogModule
{
    public class MoogHandler : ManagedMonoBehaviour, MoogHandler_API
    {
        public ModuleStatusHandler<Moog_ModuleSubStatuses> stateTracker { get; private set; }

        private MoogHostSideBridge _daemon;
        private MachineSettings _connectParameters;

        // unity components
        private DaemonsHandler _daemonsHandler;
        private TrajectoryMakerForMoogTest _controller;
        private DebugTabHandler _debugTabHandler;

        // ########################################################################################

        public override void ManagedAwake()
        {
            _daemonsHandler = GetComponent<DaemonsHandler>();
            _controller = GetComponent<TrajectoryMakerForMoogTest>();
            _debugTabHandler = GetComponent<DebugTabHandler>();

            _connectParameters = new MachineSettings
            {
                StartPosition = new DofParameters { Heave = -0.22f },
                MaxAcceleration = 2.0,
                HOST_IP = "192.168.2.3",
                HOST_PORT = "16386",
                MBC_IP = "192.168.2.1",
                MBC_PORT = "16384"
            };

            _debugTabHandler.testBtn1Clicked += (eventObj) => Engage();     // test
            _debugTabHandler.testBtn2Clicked += (eventObj) => TestMethod(); // test
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
            //_controller.OnPositionChanged += (coordinate) => MoveToPoint(new MoveToPointParameters { Coordinate = coordinate });    //test

            // here communicator and process start
            daemonWrapper.Start();                    // <----------
        }

        // ########################################################################################

        public void Connect(MachineSettings parameters)
        {
        }

        public void Engage()
        {
            _daemon.Engage();
        }

        public void Disengage()
        {
            _daemon.Disengage();
        }

        public void Reset()
        {
        }

        public void MoveToPoint(MoveToPointParameters parameters)
        {
            _daemon.MoveToPoint(parameters);
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

        private void OnMachineStateChanged(MoogRealTimeState state)
        {
            /*if (state == MachineState.Ready || state == MachineState.Engaged)
                stateTracker.UpdateSubStatus(Moog_ModuleSubStatuses.Machine, SubStatusState.Complete);
            else if (state == MachineState.Initializing || state == MachineState.Standby || state == MachineState.Parking)
                stateTracker.UpdateSubStatus(Moog_ModuleSubStatuses.Machine, SubStatusState.StillNotSet);
            else
                stateTracker.UpdateSubStatus(Moog_ModuleSubStatuses.Machine, SubStatusState.Failed);*/
        }

        private void TestMethod()
        {
            var centerPoint = new DofParameters { Heave = -0.22f };
            var destinationPoint = new DofParameters { Heave = -0.22f, Surge = 0.13f, Sway = -0.02f };
            var trajectoryTypeSettings = new TrajectoryTypeSettings { Linear = new TrajectoryTypeSettings_Linear { } };
            var trajectoryProfileSettings = new TrajectoryProfileSettings { CDF = new TrajectoryProfileSettings_CDF { Sigmas = 3 } };
            var trajectoryType = TrajectoryType.Linear;
            var trajectoryProfile = TrajectoryProfile.CDF;
            var delayHandling = DelayCompensationStrategy.Ignore;


            var firstTrajectorySettings = new MoveByTrajectoryParameters
            {
                StartPoint = centerPoint,
                EndPoint = destinationPoint,
                MovementDuration = TimeSpan.FromSeconds(1),
                TrajectoryTypeSettings = trajectoryTypeSettings,
                TrajectoryProfileSettings = trajectoryProfileSettings,
                DelayHandling = delayHandling,
                TrajectoryType = trajectoryType,
                TrajectoryProfile = trajectoryProfile
            };

            var secondTrajectorySettings = new MoveByTrajectoryParameters
            {
                StartPoint = destinationPoint,
                EndPoint = centerPoint,
                MovementDuration = TimeSpan.FromSeconds(2),
                TrajectoryTypeSettings = trajectoryTypeSettings,
                TrajectoryProfileSettings = trajectoryProfileSettings,
                DelayHandling = delayHandling,
                TrajectoryType = trajectoryType,
                TrajectoryProfile = trajectoryProfile
            };

            IEnumerator TestCoroutine()
            {
                _daemon.MoveByTrajectory(firstTrajectorySettings);
                yield return new WaitForSeconds(1f + 3f);
                _daemon.MoveByTrajectory(secondTrajectorySettings);
                yield return new WaitForSeconds(2f + 3f);
                _daemon.Disengage();
            }


            StartCoroutine(TestCoroutine());
        }
    }
}