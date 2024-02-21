using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using CommonUtilitiesNamespace;

namespace AudioControl
{
    public class AudioManager
    {
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

        public AudioManager()
        {
            unifiedWaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2); // 32 bit IEEFloat: 44100Hz 2 channels
            //pathToAudioFiles = @"..\..\MOCU-UnityCore\Assets\Audio";          // todo: move it to config file later
            audioFileNames = new() { "test.mp3", "test2.mp3" };                 // todo: maybe read it from config or unity, idk

            enumerator = new();

            UpdateAudioDevices();

            incomingStream = new(direction: IntercomStreamDirection.Incoming, audioManager: this);
            outgoingStream = new(direction: IntercomStreamDirection.Outgoing, audioManager: this);

            //LoadAudioFiles();
            InitOutputDevicesDictionary();
            InitInputDevicesDictionary();
        }

        public void RespondToCommand(string response)
        {
            outputMessagesQueue.Enqueue(response);
        }


        public void ProcessCommand(string jsonCommand)
        {
            var commandName = CommonUtilities.GetSerializedObjectType(jsonCommand);


            switch (commandName)
            {
                // COMMON COMMANDS
                // {} inside of each "case" are for using variable with same name

                case "SetDevicesParameters_Command":
                    {
                        var response = SetDevicesParameters(jsonCommand);
                        RespondToCommand(response);
                        break;
                    }

                case "ChangeOutputDeviceVolume_Command":
                    {
                        var response = ChangeOutputDeviceVolume(jsonCommand);
                        RespondToCommand(response);
                        break;
                    }
                case "SendConfigs_Command":
                    {
                        var response = ApplyConfigs(jsonCommand);
                        RespondToCommand(response);
                        break;
                    }

            case "PlayAudioFile_Command":
                    try {
                        Task.Run(() => PlayAudioFile(jsonCommand));
                        RespondToCommand(CommonUtilities.SerializeJson(new GeneralResponseFromServer_Command(receivedCommand: "PlayAudioFile_Command", hasError: false)));
                    } catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        RespondToCommand(CommonUtilities.SerializeJson(new GeneralResponseFromServer_Command(receivedCommand: "PlayAudioFile_Command", hasError: true)));
                    }
                    break;
                    

                case "GetAudioDevices_Command":
                    {
                        var response = GetAudioDevicesAsJson(jsonCommand);
                        RespondToCommand(response);
                        break;
                    }

                // INTERCOM COMMANDS
                case "StartIntercomStream_ResearcherToParticipant_Command":
                    outgoingStream.StartStream();
                    RespondToCommand(CommonUtilities.SerializeJson(new GeneralResponseFromServer_Command(receivedCommand: "StartIntercomStream_ResearcherToParticipant_Command", hasError: false)));
                    break;

                case "StartIntercomStream_ParticipantToResearcher_Command":
                    incomingStream.StartStream();
                    RespondToCommand(CommonUtilities.SerializeJson(new GeneralResponseFromServer_Command(receivedCommand: "StartIntercomStream_ParticipantToResearcher_Command", hasError: false)));
                    break;

                case "StopIntercomStream_ResearcherToParticipant_Command":
                    outgoingStream.StopStream();
                    RespondToCommand(CommonUtilities.SerializeJson(new GeneralResponseFromServer_Command(receivedCommand: "StopIntercomStream_ResearcherToParticipant_Command", hasError: false)));
                    break;

                case "StopIntercomStream_ParticipantToResearcher_Command":
                    incomingStream.StopStream();
                    RespondToCommand(CommonUtilities.SerializeJson(new GeneralResponseFromServer_Command(receivedCommand: "StopIntercomStream_ParticipantToResearcher_Command", hasError: false)));
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
                    RespondToCommand(CommonUtilities.SerializeJson(new GeneralResponseFromServer_Command(receivedCommand: "NotYetImplemented_Command", hasError: true)));
                    break;


                default:
                    RespondToCommand(CommonUtilities.SerializeJson(new GeneralResponseFromServer_Command(receivedCommand: "Unknown_Command", hasError: true)));
                    break;
            }
        }

        private void InitOutputDevicesDictionary()
        {
            audioOutputsDictionary = new();

            foreach (var device in outputDevices)
            {
                audioOutputsDictionary.Add( device.FriendlyName, new AudioOutputDevice(device, unifiedWaveFormat) );
            }
        }

        private void InitInputDevicesDictionary()
        {
            audioInputsDictionary = new();

            foreach (var device in inputDevices)
            {
                audioInputsDictionary.Add( device.FriendlyName, new AudioInputDevice(device, unifiedWaveFormat) );
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

        private string ApplyConfigs(string jsonCommand)
        {
            try
            {
                var obj = CommonUtilities.DeserializeJson<SendConfigs_Command>(jsonCommand);
                pathToAudioFiles = obj.UnityAudioDirectory;

                LoadAudioFiles();   // todo: move to other place

                return CommonUtilities.SerializeJson(new GeneralResponseFromServer_Command(receivedCommand: "SendConfigs_Command", hasError: false));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return CommonUtilities.SerializeJson(new GeneralResponseFromServer_Command(receivedCommand: "SendConfigs_Command", hasError: true));
            }
        }

        /*private void UnsubscribeDevice_TestMethod(ISampleProvider sampleProvider)
        {
            foreach (var test in audioOutputsDictionary.Keys.ToList())
            {
                audioOutputsDictionary[test].mixer.RemoveMixerInput(sampleProvider);
            }
        }*/


        private string SetDevicesParameters(string jsonCommand)
        {
            try
            {
                var obj = CommonUtilities.DeserializeJson<SetDevicesParameters_Command>(jsonCommand);

                outgoingStream.UpdateAudioDevices(
                    audioInputDeviceName: obj.AudioInputDeviceNameResearcher,
                    audioOutputDeviceName: obj.AudioOutputDeviceNameParticipant,
                    audioOutputDeviceVolume: obj.AudioOutputDeviceVolumeParticipant
                );

                incomingStream.UpdateAudioDevices(
                    audioInputDeviceName: obj.AudioInputDeviceNameParticipant,
                    audioOutputDeviceName: obj.AudioOutputDeviceNameResearcher,
                    audioOutputDeviceVolume: obj.AudioOutputDeviceVolumeResearcher
                );

                return CommonUtilities.SerializeJson(new GeneralResponseFromServer_Command(receivedCommand: "SetDevicesParameters_Command", hasError: false));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return CommonUtilities.SerializeJson(new GeneralResponseFromServer_Command(receivedCommand: "SetDevicesParameters_Command", hasError: true));
            }
        }

        private string ChangeOutputDeviceVolume(string jsonCommand)
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
        }

        private void UpdateAudioDevices()
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

        private string GetAudioDevicesAsJson(string jsonCommand)
        {
            var obj = CommonUtilities.DeserializeJson<GetAudioDevices_Command>(jsonCommand);
            if (obj.DoUpdate) UpdateAudioDevices();

            return CommonUtilities.SerializeJson(new ResponseFromServer_GetAudioDevices_Command(
                inputDevices: inputDevices.Select(device => device.FriendlyName).ToList(),
                outputDevices: outputDevices.Select(device => device.FriendlyName).ToList()
            ));
        }

        /// <summary>
        /// Copies pre-prepared and unified audio to a buffer subscribed to the mixer.
        /// If something is being played at the moment of calling the function, it purposely cuts it off and puts a newer one
        /// </summary>
        private void PlayAudioFile(string jsonCommand)
        {
            var commandData = CommonUtilities.DeserializeJson<PlayAudioFile_Command>(jsonCommand);

            var audioData = preLoadedAudioFiles[commandData.AudioFileName];
            var buffer = audioOutputsDictionary[commandData.AudioOutputDeviceName].bufferForSingleAudioPlay;

            buffer.ClearBuffer();
            buffer.AddSamples(audioData, 0, audioData.Length);
        }
    }
}
