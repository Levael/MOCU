using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Reflection;

using DaemonsNamespace.Common;


namespace AudioControl
{
    /// <summary>
    /// Manages and tracks the parameters and state changes of audio devices,
    /// including both input and output devices for researcher and participant.
    /// 
    /// This class handles updates to device selection and volume levels,
    /// ensures synchronization with external systems,
    /// and notifies other components about changes and errors.
    /// </summary>
    public class AudioDevicesParameters
    {
        public event Action? AudioDeviceHasChanged;     // (for inner notification)     Triggered to update intercom settings when a change in audio devices is detected
        public event Action? SendLatestData;            // (for outter notification)    Triggered to update list of available options on client side


        private MMDeviceEnumerator enumerator;
        public WaveFormat unifiedWaveFormat;
        private NotificatorForAudioDevicesParameters notificator;

        
        public Dictionary<string, AudioOutputDevice> audioOutputsDictionary { get; private set; }
        public Dictionary<string, AudioInputDevice> audioInputsDictionary { get; private set; }

        
        public AudioOutputDevice? audioOutputDevice_Researcher { get; private set; }
        public AudioOutputDevice? audioOutputDevice_Participant { get; private set; }
        public AudioInputDevice? audioInputDevice_Researcher { get; private set; }
        public AudioInputDevice? audioInputDevice_Participant { get; private set; }
        



        public AudioDevicesParameters()
        {
            enumerator = new MMDeviceEnumerator();
            notificator = new NotificatorForAudioDevicesParameters(this);
            unifiedWaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2); // 32 bit IEEFloat: 44100Hz 2 channels

            UpdateDictionaries();   // before 'notificator' registation
            enumerator.RegisterEndpointNotificationCallback(notificator);
        }

        private void UpdateDictionaries()
        {
            var inputDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            var outputDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            audioInputsDictionary = inputDevices.ToDictionary(device => device.FriendlyName, device => new AudioInputDevice(device, unifiedWaveFormat));
            audioOutputsDictionary = outputDevices.ToDictionary(device => device.FriendlyName, device => new AudioOutputDevice(device, unifiedWaveFormat));

            CleanUpAfterDictionariesUpdate();
            SendLatestData?.Invoke();
        }


        private void CleanUpAfterDictionariesUpdate()
        {
            // Check if any device in use has been disconnected (removed from the dictionary)
            //
            // It looks terrible, maybe I'll change it in the future.
            // In a nutshell: we go through all the devices and if they are disconnected, we write null.
            // 'GetAudioData' methods is needed in order to easly go through 4 Audio_X_Devices

            var audioData = GetAudioData();
            var audioDevicesInfo = audioData.audioDevicesInfo;
            if (audioDevicesInfo == null) return;   // todo: think about this line later

            foreach (FieldInfo field in typeof(AudioDevicesInfo).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.FieldType == typeof(string))
                {
                    string? value = (string?)field.GetValue(audioDevicesInfo);
                    if (!string.IsNullOrEmpty(value) && !audioOutputsDictionary.ContainsKey(value) && !audioInputsDictionary.ContainsKey(value))
                    {
                        field.SetValue(audioDevicesInfo, null);
                        AudioDeviceHasChanged?.Invoke();
                    } 
                }
            }
        }


        /// <summary>
        /// Updates the state of audio devices based on the provided parameters. Notifies other components if changes occurred.
        /// </summary>
        /// <param name="updatedParameters">New audio device settings to be applied.</param>
        public void UpdateParameters(AudioDevicesInfo updatedParameters)
        {
            int errorsOccurred = 0;
            var deviceChangesOccurred = 0;
            var volumeChangesOccurred = 0;  // not in use

            // example of naming: UAODNR = UpdatedAudioOutputDeviceNameResearcher

            var UAODNR = updatedParameters.audioOutputDeviceName_Researcher;
            if (audioOutputDevice_Researcher?.name != UAODNR)
            {
                try
                {
                    if (String.IsNullOrEmpty(UAODNR) || !audioOutputsDictionary.ContainsKey(UAODNR))
                        audioOutputDevice_Researcher = null;
                    else
                        audioOutputDevice_Researcher = audioOutputsDictionary[UAODNR];

                    deviceChangesOccurred++;
                }
                catch
                {
                    DaemonsUtilities.ConsoleError($"audioOutputDevice_Researcher: {audioOutputDevice_Researcher}");
                    audioOutputDevice_Researcher = null;
                    errorsOccurred++;
                }
            }

            var UAODNP = updatedParameters.audioOutputDeviceName_Participant;
            if (audioOutputDevice_Participant?.name != UAODNP)
            {
                try
                {
                    if (String.IsNullOrEmpty(UAODNP) || !audioOutputsDictionary.ContainsKey(UAODNP))
                        audioOutputDevice_Participant = null;
                    else
                        audioOutputDevice_Participant = audioOutputsDictionary[UAODNP];

                    deviceChangesOccurred++;
                }
                catch
                {
                    DaemonsUtilities.ConsoleError($"audioOutputDevice_Participant: {audioOutputDevice_Participant}");
                    audioOutputDevice_Participant = null;
                    errorsOccurred++;
                }
            }

            var UAIDNR = updatedParameters.audioInputDeviceName_Researcher;
            if (audioInputDevice_Researcher?.name != UAIDNR)
            {
                try
                {
                    if (String.IsNullOrEmpty(UAIDNR) || !audioInputsDictionary.ContainsKey(UAIDNR))
                        audioInputDevice_Researcher = null;
                    else
                        audioInputDevice_Researcher = audioInputsDictionary[UAIDNR];

                    deviceChangesOccurred++;
                }
                catch
                {
                    DaemonsUtilities.ConsoleError($"audioInputDevice_Researcher: {audioInputDevice_Researcher}");
                    audioInputDevice_Researcher = null;
                    errorsOccurred++;
                }
            }

            var UAIDNP = updatedParameters.audioInputDeviceName_Participant;
            if (audioInputDevice_Participant?.name != UAIDNP)
            {
                try
                {
                    if (String.IsNullOrEmpty(UAIDNP) || !audioInputsDictionary.ContainsKey(UAIDNP))
                        audioInputDevice_Participant = null;
                    else
                        audioInputDevice_Participant = audioInputsDictionary[UAIDNP];

                    deviceChangesOccurred++;
                }
                catch
                {
                    DaemonsUtilities.ConsoleError($"audioInputDevice_Participant: {audioInputDevice_Participant}");
                    audioInputDevice_Participant = null;
                    errorsOccurred++;
                }
            }

            var UAODVR = updatedParameters.audioOutputDeviceVolume_Researcher;
            if (audioOutputDevice_Researcher != null && audioOutputDevice_Researcher.volume != UAODVR)
            {
                try
                {
                    var value = UAODVR;
                    if (value != null)
                    {
                        audioOutputDevice_Researcher.volume = (float)value;
                        volumeChangesOccurred++;
                    }
                        
                }
                catch
                {
                    DaemonsUtilities.ConsoleError($"audioOutputDevice_Researcher volum: {audioOutputDevice_Researcher}");
                    errorsOccurred++;
                }
            }

            var UAODVP = updatedParameters.audioOutputDeviceVolume_Participant;
            if (audioOutputDevice_Participant != null && audioOutputDevice_Participant.volume != UAODVP)
            {
                try
                {
                    var value = UAODVP;
                    if (value != null)
                    {
                        audioOutputDevice_Participant.volume = (float)value;
                        volumeChangesOccurred++;
                    }
                }
                catch
                {
                    DaemonsUtilities.ConsoleError($"audioOutputDevice_Participant volum: {audioOutputDevice_Participant}");
                    errorsOccurred++;
                }
            }

            var UAIDVR = updatedParameters.audioInputDeviceVolume_Researcher;
            if (audioInputDevice_Researcher != null && audioInputDevice_Researcher.volume != UAIDVR)
            {
                try
                {
                    var value = UAIDVR;
                    if (value != null)
                    {
                        audioInputDevice_Researcher.volume = (float)value;
                        volumeChangesOccurred++;
                    }
                }
                catch
                {
                    DaemonsUtilities.ConsoleError($"audioInputDevice_Researcher volum: {audioInputDevice_Researcher}");
                    errorsOccurred++;
                }
            }

            var UAIDVP = updatedParameters.audioInputDeviceVolume_Participant;
            if (audioInputDevice_Participant != null && audioInputDevice_Participant.volume != UAIDVP)
            {
                try
                {
                    var value = UAIDVP;
                    if (value != null)
                    {
                        audioInputDevice_Participant.volume = (float)value;
                        volumeChangesOccurred++;
                    }
                }
                catch
                {
                    DaemonsUtilities.ConsoleError($"audioInputDevice_Participant volum: {audioInputDevice_Participant}");
                    errorsOccurred++;
                }
            }



            if (deviceChangesOccurred > 0 || errorsOccurred > 0)
                AudioDeviceHasChanged?.Invoke();

            SendLatestData?.Invoke();
        }

        public UnifiedAudioDataPacket GetAudioData()
        {
            var audioDevicesInfo = new AudioDevicesInfo()
            {
                audioOutputDeviceName_Researcher = audioOutputDevice_Researcher?.name,
                audioOutputDeviceName_Participant = audioOutputDevice_Participant?.name,
                audioInputDeviceName_Researcher = audioInputDevice_Researcher?.name,
                audioInputDeviceName_Participant = audioInputDevice_Participant?.name,

                audioOutputDeviceVolume_Researcher = audioOutputDevice_Researcher?.volume,
                audioOutputDeviceVolume_Participant = audioOutputDevice_Participant?.volume,
                audioInputDeviceVolume_Researcher = audioInputDevice_Researcher?.volume,
                audioInputDeviceVolume_Participant = audioInputDevice_Participant?.volume,
            };

            var inputAudioDevices = audioInputsDictionary != null ? new List<string>(audioInputsDictionary.Keys) : null;
            var outputAudioDevices = audioOutputsDictionary != null ? new List<string>(audioOutputsDictionary.Keys) : null;


            return new UnifiedAudioDataPacket(
                audioDevicesInfo: audioDevicesInfo,
                inputAudioDevices: inputAudioDevices,
                outputAudioDevices: outputAudioDevices
            );
        }



        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            Console.WriteLine($"OnDeviceStateChanged. deviceId: {deviceId}, newState: {newState}");

            UpdateDictionaries();
        }
    }
}
