using DaemonsRelated;
using DaemonsRelated.DaemonPart;
using InterprocessCommunication;
using System.Linq;
using System;


namespace AudioModule.Daemon
{
    public class AudioDaemonSideBridge : AudioHost_API, IHostAPI
    {
        public event Action<IEnumerable<PlayAudioClipCommand>> PlayAudioClips;
        public event Action<IEnumerable<AudioDeviceData>> UpdateAudioDevices;
        public event Action<IEnumerable<AudioClipData>> UpdateAudioClips;
        public event Action<IEnumerable<AudioIntercomData>> StartIntercoms;
        public event Action<IEnumerable<AudioIntercomData>> StopIntercoms;

        public event Action<string> TerminateDaemon;

        private IInterprocessCommunicator _communicator;

        public AudioDaemonSideBridge(IInterprocessCommunicator communicator)
        {
            _communicator = communicator;

            _communicator.MessageReceived       += message => HandleIncomingMessage(message);
            _communicator.MessageSent           += message => Console.WriteLine($"Sent message to host: {message}");
            _communicator.ConnectionEstablished += message => Console.WriteLine($"Connection established: {message}");
            _communicator.ConnectionBroked      += message => Console.WriteLine($"Connection broked: {message}");
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

        // ########################################################################################

        private void HandleIncomingMessage(string message)
        {
            try
            {
                Console.WriteLine($"Got message from host: {message}"); // temp

                var dataTransferObject = JsonHelper.DeserializeJson<AudioDataTransferObject>(message);

                // CUSTOM MESSAGE
                if (!String.IsNullOrEmpty(dataTransferObject.CustomMessage))
                {
                    Console.WriteLine($"Custom message in 'HandleIncomingMessage': {dataTransferObject.CustomMessage}");
                    _communicator.SendMessage($"got message from u: {dataTransferObject.CustomMessage}");   // temp
                }
                    

                // TERMINATION COMMAND
                if (dataTransferObject.DoTerminateTheDaemon)
                {
                    TerminateDaemon?.Invoke("Got command from host to terminate the daemon");
                    Console.WriteLine("shouldn't see that");
                }
                    

                // CLIP CHANGES
                if (dataTransferObject.ClipChanges.Any())
                    UpdateAudioClips?.Invoke(dataTransferObject.ClipChanges);

                // DEVICE CHANGES
                if (dataTransferObject.DeviceChanges.Any())
                    UpdateAudioDevices?.Invoke(dataTransferObject.DeviceChanges);

                // PLAY CLIP COMMANDS
                if (dataTransferObject.PlayClipCommands.Any())
                    PlayAudioClips?.Invoke(dataTransferObject.PlayClipCommands);

                // INTERCOM COMMANDS
                if (dataTransferObject.IntercomCommands.Any())
                {
                    var isOnTrue = dataTransferObject.IntercomCommands.Where(intercom => intercom.isOn);
                    var isOnFalse = dataTransferObject.IntercomCommands.Where(intercom => !intercom.isOn);

                    if (isOnTrue.Any())
                        StartIntercoms?.Invoke(isOnTrue);

                    if (isOnFalse.Any())
                        StopIntercoms?.Invoke(isOnFalse);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'HandleIncomingMessage': {ex.Message}");
            }
        }
    }
}