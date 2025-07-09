using System.IO;
using UnityEngine;
using UnityEngine.InputSystem.XR;

using InterprocessCommunication;
using MoogModule;
using System;


namespace ChartsModule
{
    public class ChartsHandler : ManagedMonoBehaviour
    {
        // todo: take from config
        // if you need to specify a specific path. Otherwise, it will save it to a temporary storage, like 'AppData'
        private string _outputPath = @"C:\Users\Levael\Downloads";


        private ChartsHostSideBridge _daemon;

        // unity components
        private DaemonsHandler _daemonsHandler;
        private DebugTabHandler _debugTabHandler;

        public Action<Texture2D> StaticChartUpdated;

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
            _daemon.ChartImageGenerated += GotStaticChart;
            _daemon.Test();

            //communicator.ConnectionEstablished += message => stateTracker.UpdateSubStatus(Moog_ModuleSubStatuses.Communicator, SubStatusState.Complete);
            //communicator.ConnectionBroked += message => stateTracker.UpdateSubStatus(Moog_ModuleSubStatuses.Communicator, SubStatusState.Failed);

            daemonWrapper.Start();                    // <----------
        }

        // .............................

        public void StaticChart(ChartData parameters)
        {
            _daemon.GenerateChartAsImage(parameters);
        }

        public void InteractiveChart(ChartData parameters)
        {
            _daemon.GenerateChartAsForm(parameters);
        }

        // .............................

        private void GotStaticChart(string pathToImage)
        {
            try
            {
                StaticChartUpdated?.Invoke(ParsePngToTexture2D(pathToImage));
            }
            catch (Exception ex)
            {
                Debug.Log($"Couldn't 'ParsePngToTexture2D' in 'GotStaticChart' in 'ChartsHandler': {ex}");
            }
        }

        private ChartData ParseMoogFeedback(MoogFeedback data)
        {
            // todo: save once data from Moog to json file to further dabug
            return null;
        }

        private Texture2D ParsePngToTexture2D(string fullPathToImage)
        {
            // Read all file bytes
            byte[] pngBytes = File.ReadAllBytes(fullPathToImage);

            // Create a placeholder texture. Size (2×2) will be replaced by the real one.
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: true);

            // Decode PNG into the texture. 'true' is for memory saving (optimization only)
            texture.LoadImage(pngBytes, true);

            return texture;
        }
    }
}