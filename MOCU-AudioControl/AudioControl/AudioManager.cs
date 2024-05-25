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

            audioDevicesParameters.SendResponseToUnity += RespondToCommand_UpdateDevicesParameters;
            audioDevicesParameters.AudioDeviceHasChanged += UpdateIntercom;

            incomingStream = new(direction: IntercomStreamDirection.Incoming);
            outgoingStream = new(direction: IntercomStreamDirection.Outgoing);

            commandsHandlers = new()
            {
                { "UpdateDevicesParameters_Command", UpdateDevicesParameters_CommandHandler },
                { "SendConfigs_Command", SendConfigs_CommandHandler },
                { "PlayAudioFile_Command", PlayAudioFile_CommandHanler },
                { "GetAudioDevices_Command", GetAudioDevices_CommandHandler },
            };
        }


        


        public void ProcessCommand(string jsonCommand)
        {
            try
            {
                var commandName = CommonClientServerMethods.GetSerializedObjectType(jsonCommand);

                if (commandName == null || !commandsHandlers.ContainsKey(commandName))
                    throw new Exception("Command name is 'null' or not in 'commandsHandlers' dictionary");

                commandsHandlers[commandName].Invoke(jsonCommand);
                DeamonsUtilities.ConsoleInfo($"Command '{commandName}' was executed");
            }
            catch (Exception ex)
            {
                DeamonsUtilities.ConsoleError($"Error while 'ProcessCommand': {ex}");
            }


            /*switch (commandName)
            {
                // COMMON COMMANDS
                // '{}' inside of each "case" are for using variable with same name

                case "UpdateDevicesParameters_Command":
                    {
                        UpdateDevicesParameters(jsonCommand);
                        Console.WriteLine("UpdateDevicesParameters_Command");
                        *//*var response = SetDevicesParameters(jsonCommand);
                        RespondToCommand(response);*//*
                        break;
                    }

                *//*case "ChangeOutputDeviceVolume_Command":
                    {
                        var response = ChangeOutputDeviceVolume(jsonCommand);
                        RespondToCommand(response);
                        Console.WriteLine("ChangeOutputDeviceVolume_Command");
                        break;
                    }*//*
                case "SendConfigs_Command":
                    {
                        var response = ApplyConfigs(jsonCommand);
                        RespondToCommand(response);
                        Console.WriteLine("SendConfigs_Command");
                        break;
                    }

            case "PlayAudioFile_Command":
                    try {
                        Task.Run(() => PlayAudioFile(jsonCommand));
                        Console.WriteLine("PlayAudioFile_Command");
                        //RespondToCommand(CommonUtilities.SerializeJson(new GeneralResponseFromServer_Command(receivedCommand: "PlayAudioFile_Command", hasError: false)));
                    } catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        RespondToCommand(CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "PlayAudioFile_Command", hasError: true)));
                    }
                    break;
                    

                case "GetAudioDevices_Command":
                    {
                        var response = GetAudioDevicesAsJson(jsonCommand);
                        RespondToCommand(response);
                        Console.WriteLine("GetAudioDevices_Command");
                        break;
                    }

                // INTERCOM COMMANDS
                case "StartIntercomStream_ResearcherToParticipant_Command":
                    outgoingStream.StartStream();
                    Console.WriteLine("StartIntercomStream_ResearcherToParticipant_Command");
                    //RespondToCommand(CommonUtilities.SerializeJson(new GeneralResponseFromServer_Command(receivedCommand: "StartIntercomStream_ResearcherToParticipant_Command", hasError: false)));
                    break;

                case "StartIntercomStream_ParticipantToResearcher_Command":
                    incomingStream.StartStream();
                    Console.WriteLine("StartIntercomStream_ParticipantToResearcher_Command");
                    //RespondToCommand(CommonUtilities.SerializeJson(new GeneralResponseFromServer_Command(receivedCommand: "StartIntercomStream_ParticipantToResearcher_Command", hasError: false)));
                    break;

                case "StopIntercomStream_ResearcherToParticipant_Command":
                    outgoingStream.StopStream();
                    Console.WriteLine("StopIntercomStream_ResearcherToParticipant_Command");
                    //RespondToCommand(CommonUtilities.SerializeJson(new GeneralResponseFromServer_Command(receivedCommand: "StopIntercomStream_ResearcherToParticipant_Command", hasError: false)));
                    break;

                case "StopIntercomStream_ParticipantToResearcher_Command":
                    incomingStream.StopStream();
                    Console.WriteLine("StopIntercomStream_ParticipantToResearcher_Command");
                    //RespondToCommand(CommonUtilities.SerializeJson(new GeneralResponseFromServer_Command(receivedCommand: "StopIntercomStream_ParticipantToResearcher_Command", hasError: false)));
                    break;

                // CHANGE INPUT DEVICE COMMANDS
                case "SetResearcherAudioInputDevice_Command":
                case "SetParticipantAudioInputDevice_Command":
                case "DisconnectResearcherAudioInputDevice_Command":
                case "DisconnectParticipantAudioInputDevice_Command":

                // CHANGE OUTPUT DEVICE COMMANDS
                case "SetResearcherAudioOutputDevice_Command":
                case "SetParticipantAudioOutputDevice_Command":
                case "DisconnectResearcherAudioOutputDevice_Command":
                case "DisconnectParticipantAudioOutputDevice_Command":
                {
                    var fullJsonResponse = CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "NotYetImplemented_Command", hasError: true));
                    RespondToCommand(fullJsonResponse);
                    Console.WriteLine("NotYetImplemented_Command");
                    break;
                } 


                default:
                {
                    var fullJsonResponse = CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "Unknown_Command", hasError: true));
                    RespondToCommand(fullJsonResponse);
                    Console.WriteLine("Unknown_Command");
                    break;
                }
                    
            }*/
        }

        // INNER
        private void RespondToCommand(string? response)
        {
            if (response == null)
            {
                string callerMethodName = CommonUtilities.GetCallerMethodName();
                Console.WriteLine($"To method 'RespondToCommand' has past 'null' from method '{callerMethodName}'");
                return;
            }

            outputMessagesQueue.Enqueue(response);
        }

        private void InitOutputDevicesDictionary()
        {
            audioOutputsDictionary = new();

            foreach (var device in outputDevices)
            {
                audioOutputsDictionary.Add(device.FriendlyName, new AudioOutputDevice(device, unifiedWaveFormat));
            }
        }

        private void InitInputDevicesDictionary()
        {
            audioInputsDictionary = new();

            foreach (var device in inputDevices)
            {
                audioInputsDictionary.Add(device.FriendlyName, new AudioInputDevice(device, unifiedWaveFormat));
            }
        }

        private void LoadAudioFiles()
        {
            preLoadedAudioFiles = new();

            foreach (var audioFileName in audioFileNames)
            {
                using var reader = new AudioFileReader(Path.Combine(pathToAudioFiles, audioFileName));
                var resampler = new WdlResamplingSampleProvider(reader, unifiedWaveFormat.SampleRate);
                var sampleProvider = resampler.ToStereo();

                var wholeFile = new List<byte>();

                float[] readBuffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];
                byte[] byteBuffer = new byte[readBuffer.Length * 4];
                int samplesRead;

                while ((samplesRead = sampleProvider.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    Buffer.BlockCopy(readBuffer, 0, byteBuffer, 0, samplesRead * 4);
                    wholeFile.AddRange(byteBuffer.Take(samplesRead * 4));
                }

                preLoadedAudioFiles.Add(audioFileName, wholeFile.ToArray());
            }
        }


        // COMMAND HANDLERS

        




        





        private void UpdateIntercom()
        {
            incomingStream.UpdateDevices(
                audioInputDevice: audioDevicesParameters.audioInputDevice_Participant,
                audioOutputDevice: audioDevicesParameters.audioOutputDevice_Researcher
            );

            outgoingStream.UpdateDevices(
                audioInputDevice: audioDevicesParameters.audioInputDevice_Researcher,
                audioOutputDevice: audioDevicesParameters.audioOutputDevice_Participant
            );
        }


        /*private string ChangeOutputDeviceVolume(string jsonCommand)
        {
            try
            {
                var obj = CommonUtilities.DeserializeJson<ChangeOutputDeviceVolume_Command>(jsonCommand);

                audioOutputsDictionary[obj.Name].volume = obj.Volume;

                return CommonUtilities.SerializeJson(new GeneralResponseFromServer_Command(receivedCommand: "ChangeOutputDeviceVolume_Command", hasError: false));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return CommonUtilities.SerializeJson(new GeneralResponseFromServer_Command(receivedCommand: "ChangeOutputDeviceVolume_Command", hasError: true));
            }
        }*/

        private void UpdateMMDeviceCollections()
        {
            inputDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            outputDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
        }

        public MMDevice GetDeviceByItsName(string deviceName, MMDeviceCollection devices)
        {
            int index = -1;

            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i].FriendlyName == deviceName)
                {
                    index = i;
                    break;
                }
            }

            return devices[index];
        }

        // TODO: change to regular response and put data into payLoadObject
        /*private string GetAudioDevicesAsJson(string jsonCommand)
        {
            try
            {
                var obj = CommonUtilities.DeserializeJson<GetAudioDevices_Command>(jsonCommand);
                if (obj.DoUpdate) UpdateMMDeviceCollections();

                var responseData = new GetAudioDevices_ResponseData(
                    inputDevices: inputDevices.Select(device => device.FriendlyName).ToList(),
                    outputDevices: outputDevices.Select(device => device.FriendlyName).ToList()
                );

                return CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "GetAudioDevices_Command", hasError: false, extraData: responseData));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "GetAudioDevices_Command", hasError: true));
            }
        }*/

        /// <summary>
        /// Copies pre-prepared and unified audio to a buffer subscribed to the mixer.
        /// If something is being played at the moment of calling the function, it purposely cuts it off and puts a newer one
        /// </summary>
        private void PlayAudioFile(PlayAudioFile_Command commandData)
        {
            var audioData = preLoadedAudioFiles[commandData.AudioFileName];
            var buffer = audioOutputsDictionary[commandData.AudioOutputDeviceName].bufferForSingleAudioPlay;

            buffer.ClearBuffer();
            buffer.AddSamples(audioData, 0, audioData.Length);
        }
    }
}
