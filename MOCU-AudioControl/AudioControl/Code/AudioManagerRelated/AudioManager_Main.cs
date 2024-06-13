using DaemonsNamespace.InterprocessCommunication;


namespace AudioControl
{
    public partial class AudioManager : AbstractDaemonProgramManager
    {
        private AudioDevicesParameters audioDevicesParameters;

        public IntercomStream incomingStream;
        public IntercomStream outgoingStream;

        public string? pathToAudioFiles;
        public Dictionary<string, byte[]>? preLoadedAudioFiles;
        public List<string>? audioFileNames;
        


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

    }
}
