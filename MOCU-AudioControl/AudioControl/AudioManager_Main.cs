using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

using UnityDeamonsCommon;
using DeamonsNamespace.Common;
using DeamonsNamespace.InterprocessCommunication;

namespace AudioControl
{
    // Main part

    public partial class AudioManager
    {
        private AudioDevicesParameters audioDevicesParameters;

        private MMDeviceEnumerator enumerator;
        public MMDeviceCollection inputDevices;
        public MMDeviceCollection outputDevices;

        public IntercomStream incomingStream;
        public IntercomStream outgoingStream;

        public string pathToAudioFiles;
        public Dictionary<string, AudioOutputDevice> audioOutputsDictionary;
        public Dictionary<string, AudioInputDevice> audioInputsDictionary;
        public Dictionary<string, byte[]> preLoadedAudioFiles;
        public List<string> audioFileNames;
        public WaveFormat unifiedWaveFormat;

        // todo: those two should be from Interface or Abstract class
        public ObservableConcurrentQueue<string> outputMessagesQueue = null;
        private Dictionary<string, Action<string>> commandsHandlers;


        public AudioManager()
        {
            unifiedWaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2); // 32 bit IEEFloat: 44100Hz 2 channels
            //pathToAudioFiles = @"..\..\MOCU-UnityCore\Assets\Audio";          // todo: move it to config file later
            audioFileNames = new() { "test.mp3", "test2.mp3" };                 // todo: maybe read it from config or unity, idk

            enumerator = new();

            UpdateMMDeviceCollections();
            InitOutputDevicesDictionary();
            InitInputDevicesDictionary();

            
            audioDevicesParameters = new(outputsDict: audioOutputsDictionary, inputsDict: audioInputsDictionary);
            audioDevicesParameters.AudioDeviceHasChanged += UpdateIntercom;

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



        /// <summary>
        /// Processes a given JSON command by identifying its type and executing the corresponding handler.
        /// If the command is not recognized or an error occurs, an error response is generated and logged.
        /// </summary>
        /// <param name="jsonCommand">The JSON string representing the command to be processed.</param>
        public void ProcessCommand(string jsonCommand)
        {
            try
            {
                var commandName = CommonClientServerMethods.GetSerializedObjectType(jsonCommand);

                if (commandName == null || !commandsHandlers.ContainsKey(commandName))
                    throw new Exception($"Command name is 'null' or not in 'commandsHandlers' dictionary. Command name: {commandName}");

                commandsHandlers[commandName].Invoke(jsonCommand);
                DeamonsUtilities.ConsoleInfo($"Command '{commandName}' was executed");
            }
            catch (Exception ex)
            {
                DeamonsUtilities.ConsoleError($"Error while 'ProcessCommand': {ex}");

                var fullJsonResponse = CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "Unknown_Command", hasError: true, errorMessage: ex.ToString()));
                RespondToCommand(fullJsonResponse);
            }
        }

        /// <summary>
        /// Enqueues the given response message for processing. 
        /// Logs an error if the response is null, including the name of the calling method.
        /// </summary>
        /// <param name="response">The response message to be enqueued. If null, an error is logged.</param>
        private void RespondToCommand(string? response)
        {
            if (response == null)
            {
                string callerMethodName = CommonUtilities.GetCallerMethodName();
                DeamonsUtilities.ConsoleError($"To method 'RespondToCommand' has past 'null' from method '{callerMethodName}'");
                return;
            }

            outputMessagesQueue.Enqueue(response);
        }

    }
}
