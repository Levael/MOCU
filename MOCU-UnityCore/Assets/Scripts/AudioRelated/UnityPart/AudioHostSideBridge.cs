using InterprocessCommunication;
using DaemonsRelated;
using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace AudioModule
{
    public class AudioHostSideBridge : AudioDaemon_API, IDaemonHostBridge
    {
        private IInterprocessCommunicator _communicator;

        public AudioHostSideBridge(IInterprocessCommunicator communicator)
        {
            _communicator = communicator;
        }

        public event Action<IEnumerable<AudioDeviceData>> AudioDevicesHaveChanged;
        public event Action<IEnumerable<AudioClipData>> AudioClipsHaveChanged;
        public event Action<IEnumerable<AudioIntercomData>> IntercomsHaveChanged;
        public event Action<IEnumerable<DaemonErrorReport>> ErrorsOccurred;

        // for daemons debug purpose only (todo: maybe refactor later)
        public event Action<string> MessageReceived;
        public event Action<string> MessageSent;

        public void PlayAudioClips(IEnumerable<PlayAudioClipCommand> playCommandsData)
        {
            throw new NotImplementedException();
        }

        public void StartCommunication()
        {
            // method is async, but there is no need to use 'await'
            _communicator.Start();
        }

        public void StartIntercoms(IEnumerable<AudioIntercomData> intercomsData)
        {
            throw new NotImplementedException();
        }

        public void StopIntercoms(IEnumerable<AudioIntercomData> intercomsData)
        {
            throw new NotImplementedException();
        }

        public void TestMethod()
        {
            _communicator.SendMessage("Hello, Daemon");
        }

        public void UpdateAudioClips(IEnumerable<AudioClipData> clipsData)
        {
            throw new NotImplementedException();
        }

        public void UpdateAudioDevices(IEnumerable<AudioDeviceData> devicesData)
        {
            throw new NotImplementedException();
        }

        private void SendMessage()
        {

        }
    }
}