using System.IO;
using UnityEngine;
using UnityEngine.InputSystem.XR;

using InterprocessCommunication;
using MoogModule;


namespace ChartsModule
{
    public class ChartsHandler : ManagedMonoBehaviour
    {
        // todo: take from config
        private string _outputPath = @"";
        private string _exePath = Path.Combine(Application.streamingAssetsPath, "Daemons", "ChartViewer.exe");


        private ChartsHostSideBridge _daemon;

        // unity components
        private DaemonsHandler _daemonsHandler;
        private DebugTabHandler _debugTabHandler;

        public override void ManagedAwake()
        {
            _daemonsHandler = GetComponent<DaemonsHandler>();
            _debugTabHandler = GetComponent<DebugTabHandler>();

            /*_debugTabHandler.testBtn1Clicked += (eventObj) => Engage();     // test
            _debugTabHandler.testBtn2Clicked += (eventObj) => TestMethod(); // test*/
        }

        public override void ManagedStart()
        {
            var daemonWrapper = _daemonsHandler.GetDaemon(DaemonType.Charts);
            var communicator = daemonWrapper.Communicator;

            _daemon = new ChartsHostSideBridge(communicator);
            _daemon.Test();

            //communicator.ConnectionEstablished += message => stateTracker.UpdateSubStatus(Moog_ModuleSubStatuses.Communicator, SubStatusState.Complete);
            //communicator.ConnectionBroked += message => stateTracker.UpdateSubStatus(Moog_ModuleSubStatuses.Communicator, SubStatusState.Failed);

            daemonWrapper.Start();                    // <----------
        }

        // .............................

        public Texture2D RegularChart(ChartParameters parameters)
        {
            return null;
        }

        public void InteractiveChart(ChartParameters parameters)
        {

        }
    }
}
