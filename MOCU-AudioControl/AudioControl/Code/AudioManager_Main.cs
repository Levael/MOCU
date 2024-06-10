using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

using UnityDaemonsCommon;
using DaemonsNamespace.Common;
using DaemonsNamespace.InterprocessCommunication;

namespace AudioControl
{
    // Main part

    public partial class AudioManager : AbstractDaemonProgramManager
    {
        private AudioDevicesParameters audioDevicesParameters;

        public IntercomStream incomingStream;
        public IntercomStream outgoingStream;

        public string pathToAudioFiles;
        public Dictionary<string, byte[]> preLoadedAudioFiles;
        public List<string> audioFileNames;
        public WaveFormat unifiedWaveFormat;


        public AudioManager()
        {
            unifiedWaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2); // 32 bit IEEFloat: 44100Hz 2 channels
            //pathToAudioFiles = @"..\..\MOCU-UnityCore\Assets\Audio";          // todo: move it to config file later
            audioFileNames = new() { "test.mp3", "test2.mp3" };                 // todo: maybe read it from config or unity, idk

            audioDevicesParameters = new();
            audioDevicesParameters.AudioDeviceHasChanged += UpdateIntercom;

            UpdateAllDevicesCollections();

            incomingStream = new(direction: IntercomStreamDirection.Incoming);
            outgoingStream = new(direction: IntercomStreamDirection.Outgoing);

            commandsHandlers = new()
            {
                { "UpdateDevicesParameters_Command", UpdateDevicesParameters_CommandHandler },
                { "SendConfigs_Command", SendConfigs_CommandHandler },
                { "PlayAudioFile_Command", PlayAudioFile_CommandHanler },
                { "GetAudioDevices_Command", GetAudioDevices_CommandHandler },

                { "StartIntercomStream_ResearcherToParticipant_Command", StartIntercomStream_R2P_CommandHandler },
                { "StartIntercomStream_ParticipantToResearcher_Command", StartIntercomStream_P2R_CommandHandler },
                { "StopIntercomStream_ResearcherToParticipant_Command", StopIntercomStream_R2P_CommandHandler },
                { "StopIntercomStream_ParticipantToResearcher_Command", StopIntercomStream_P2R_CommandHandler },
            };
        }

    }
}
