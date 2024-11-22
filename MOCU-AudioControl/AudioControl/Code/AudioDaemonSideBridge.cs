using DaemonsRelated;
using InterprocessCommunication;


namespace AudioModule.Daemon
{
    public class AudioDaemonSideBridge : AudioHost_API, IDaemonHostBridge
    {
        public event Action<IEnumerable<PlayAudioClipCommand>> PlayAudioClips;
        public event Action<IEnumerable<AudioDeviceData>> UpdateAudioDevices;
        public event Action<IEnumerable<AudioClipData>> UpdateAudioClips;
        public event Action<IEnumerable<AudioIntercomData>> StartIntercoms;
        public event Action<IEnumerable<AudioIntercomData>> StopIntercoms;

        private IInterprocessCommunicator _communicator;

        public AudioDaemonSideBridge(IInterprocessCommunicator communicator)
        {
            _communicator = communicator;
            _communicator.MessageReceived += message => Console.WriteLine($"Got message from host: {message}");
            _communicator.ConnectionEstablished += () => Console.WriteLine($"Connection established");
            _communicator.ConnectionBroked += reason => Console.WriteLine($"Connection broked: {reason}");
        }

        public void StartCommunication()
        {
            Console.WriteLine("start of AudioDaemonSideBridge StartCommunication");
            _communicator.Start();
            Console.WriteLine("end of AudioDaemonSideBridge StartCommunication");
        }

        public void AudioClipsHaveChanged(IEnumerable<AudioClipData> clipsData)
        {
            throw new NotImplementedException();
        }

        public void AudioDevicesHaveChanged(IEnumerable<AudioDeviceData> devicesData)
        {
            throw new NotImplementedException();
        }

        public void ErrorsOccurred(IEnumerable<DaemonErrorReport> errors)
        {
            throw new NotImplementedException();
        }

        public void IntercomsHaveChanged(IEnumerable<AudioIntercomData> intercomsData)
        {
            throw new NotImplementedException();
        }
    }
}
