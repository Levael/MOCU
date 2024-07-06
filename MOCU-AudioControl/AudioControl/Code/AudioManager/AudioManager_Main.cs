using DaemonsNamespace.Common;
using InterprocessCommunication;


namespace AudioControl
{
    public partial class AudioManager : IBusinessLogic_Server
    {
        public event Action<UnifiedResponseFrom_Server> SendResponse;

        private AudioDevicesParameters audioDevicesParameters;
        private Dictionary<string, Action<UnifiedCommandFrom_Client>> commandsHandlers;

        private IntercomStream incomingStream;
        private IntercomStream outgoingStream;

        private string? pathToAudioFiles;
        private Dictionary<string, byte[]>? preLoadedAudioFiles;
        private List<string>? audioFileNames;


        public AudioManager()
        {
            audioFileNames = new() { "test.mp3", "test2.mp3" }; // todo: maybe read it from config or unity, idk

            audioDevicesParameters = new AudioDevicesParameters();
            audioDevicesParameters.AudioDeviceHasChanged += UpdateIntercom;
            audioDevicesParameters.SendLatestData += SendDataToClient;

            incomingStream = new IntercomStream(direction: IntercomStreamDirection.Incoming);
            outgoingStream = new IntercomStream(direction: IntercomStreamDirection.Outgoing);

            commandsHandlers = new()
            {
                { "SetUpdatedAudioDevicesInfo_Command", UpdateDevicesParameters_CommandHandler },
                { "SetConfigurations_Command", SendConfigs_CommandHandler },
                { "PlayAudioFile_Command", PlayAudioFile_CommandHanler },

                { "StartOutgoingIntercomStream_Command", StartIntercomStream_R2P_CommandHandler },
                { "StartIncomingIntercomStream_Command", StartIntercomStream_P2R_CommandHandler },
                { "StopOutgoingIntercomStream_Command", StopIntercomStream_R2P_CommandHandler },
                { "StopIncomingIntercomStream_Command", StopIntercomStream_P2R_CommandHandler },
            };
        }


        public void ProcessCommand(UnifiedCommandFrom_Client command)
        {
            try
            {
                if (command == null || !commandsHandlers.ContainsKey(command.name))
                    throw new Exception($"Command from client is incorrect or unknown");

                commandsHandlers[command.name].Invoke(command);

                DaemonsUtilities.ConsoleInfo($"Command '{command.name}' was executed");
            }
            catch (Exception ex)
            {
                SendResponse?.Invoke(new UnifiedResponseFrom_Server(name: "CommandProcessingError", errorOccurred: true, errorMessage: ex.ToString(), errorIsFatal: false));

                DaemonsUtilities.ConsoleError($"Error while 'ProcessCommand': {ex}");
            }
        }
    }
}
