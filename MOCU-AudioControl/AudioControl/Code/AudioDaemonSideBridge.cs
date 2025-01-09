using DaemonsRelated;
using DaemonsRelated.DaemonPart;
using InterprocessCommunication;


namespace AudioModule.Daemon
{
    public class AudioDaemonSideBridge : AudioHost_API, IHostAPI
    {
        public event Action<IEnumerable<PlayAudioClipCommand>>  PlayClips;
        public event Action<IEnumerable<AudioDeviceData>>       UpdateDevicesData;
        public event Action<IEnumerable<AudioClipData>>         UpdateClipsData;
        public event Action<IEnumerable<AudioIntercomData>>     UpdateIntercomStates;
        public event Action<string>                             TerminateDaemon;

        private IInterprocessCommunicator _communicator;

        public AudioDaemonSideBridge(IInterprocessCommunicator communicator)
        {
            _communicator = communicator;

            _communicator.MessageReceived       += message => HandleIncomingMessage(message);
            //_communicator.MessageSent           += message => Console.WriteLine($"Sent message to host");
            _communicator.ConnectionEstablished += message => Console.WriteLine($"Connection established. {message}");
            _communicator.ConnectionBroked      += message => Console.WriteLine($"Connection broked. {message}");
        }

        public void StartCommunication()
        {
            _communicator.Start();
        }

        // ########################################################################################

        public void ClipsDataChanged(IEnumerable<AudioClipData> clipsData)
        {
            var audioDataTransferObject = new AudioDataTransferObject() { ClipChanges = clipsData };
            var json = JsonHelper.SerializeJson(audioDataTransferObject);
            _communicator.SendMessage(json);
        }

        public void DevicesDataChanged(IEnumerable<AudioDeviceData> devicesData)
        {
            var audioDataTransferObject = new AudioDataTransferObject() { DeviceChanges = devicesData };
            var json = JsonHelper.SerializeJson(audioDataTransferObject);
            _communicator.SendMessage(json);
        }

        public void ErrorsOccurred(IEnumerable<DaemonErrorReport> errors)
        {
            var audioDataTransferObject = new AudioDataTransferObject() { DaemonErrorReports = errors };
            var json = JsonHelper.SerializeJson(audioDataTransferObject);
            _communicator.SendMessage(json);
        }

        public void IntercomStatesChanged(IEnumerable<AudioIntercomData> intercomsData)
        {
            var audioDataTransferObject = new AudioDataTransferObject() { IntercomCommands = intercomsData };
            var json = JsonHelper.SerializeJson(audioDataTransferObject);
            _communicator.SendMessage(json);
        }

        // ########################################################################################

        private void HandleIncomingMessage(string message)
        {
            try
            {
                var dataTransferObject = JsonHelper.DeserializeJson<AudioDataTransferObject>(message);

                // CUSTOM MESSAGE
                if (!String.IsNullOrEmpty(dataTransferObject.CustomMessage))
                    Console.WriteLine($"Custom message in 'HandleIncomingMessage': {dataTransferObject.CustomMessage}");

                // TERMINATION COMMAND
                if (dataTransferObject.DoTerminateTheDaemon)
                    TerminateDaemon?.Invoke("Got command from host to terminate the daemon");

                // CLIP CHANGES
                if (dataTransferObject.ClipChanges.Any())
                    UpdateClipsData?.Invoke(dataTransferObject.ClipChanges);

                // DEVICE CHANGES
                if (dataTransferObject.DeviceChanges.Any())
                    UpdateDevicesData?.Invoke(dataTransferObject.DeviceChanges);

                // PLAY CLIP COMMANDS
                if (dataTransferObject.PlayClipCommands.Any())
                    PlayClips?.Invoke(dataTransferObject.PlayClipCommands);

                // INTERCOM COMMANDS
                if (dataTransferObject.IntercomCommands.Any())
                    UpdateIntercomStates?.Invoke(dataTransferObject.IntercomCommands);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'HandleIncomingMessage': {ex.Message}");
            }
        }
    }
}