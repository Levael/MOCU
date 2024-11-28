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

            _communicator.MessageReceived += message => { Console.WriteLine($"Got message from host: {message}"); _communicator.SendMessage($"got from u: {message}"); };
            _communicator.MessageSent += message => Console.WriteLine($"Sent message to host: {message}");
            _communicator.ConnectionEstablished += () => Console.WriteLine($"Connection established");
            _communicator.ConnectionBroked += reason => Console.WriteLine($"Connection broked: {reason}");
        }

        public void StartCommunication()
        {
            _communicator.Start();
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