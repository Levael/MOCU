using NAudio.CoreAudioApi;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System.Numerics;
using System.Reflection;

namespace AudioControl
{
    /// <summary>
    /// Manages the capture of audio data and routes it to a selected AudioOutputDevice's buffer.
    /// This class allows for dynamically switching the target AudioOutputDevice, though only one device can be active at a time.
    /// 
    /// Additionally, the AudioInputDevice is responsible for managing the wave format of the capturing device,
    /// ensuring proper audio processing and compatibility with the selected output.
    /// </summary>
    public class AudioInputDevice
    {
        public string name                      { get; private set; }
        public WaveFormat waveFormat            { get; private set; }

        private MMDevice device                 { get; set; }
        private BufferedWaveProvider? buffer    { get; set; }
        private WasapiCapture receiver          { get; set; }

        private float _volume;
        /// <summary>
        /// Currently not in use
        /// </summary>
        public float volume { get; set; }


        public AudioInputDevice(MMDevice mmDevice, WaveFormat unifiedWaveFormat)
        {
            device = mmDevice;
            name = device.FriendlyName;
            waveFormat = unifiedWaveFormat;
            buffer = null;

            receiver = new WasapiCapture(device);
            receiver.DataAvailable += OnDataAvailable;
            receiver.WaveFormat = waveFormat;
        }



        public bool StartRecording()
        {
            try
            {
                receiver.StartRecording();
                return true;
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public bool StopRecording()
        {
            try
            {
                receiver.StopRecording();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            buffer.AddSamples(e.Buffer, 0, e.BytesRecorded);

            // tried to add noise cancelling
            /*// Пороговое значение для шумоподавления
            float threshold = 0.5f; // Задайте порог в соответствии с вашими потребностями

            // Проходим по всему буферу и применяем пороговое значение для отсечения шума
            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                // Конвертируем байты в 16-битное значение
                short sample = (short)((e.Buffer[index + 1] << 8) | e.Buffer[index]);

                // Применяем шумоподавление
                float sampleFloat = sample / 32768f; // Конвертируем в значение от -1.0 до 1.0
                if (Math.Abs(sampleFloat) < threshold)
                {
                    sample = 0;
                }

                // Конвертируем обратно в байты
                e.Buffer[index] = (byte)(sample & 0xFF);
                e.Buffer[index + 1] = (byte)(sample >> 8);
            }

            // Добавляем обработанные данные в буфер
            buffer.AddSamples(e.Buffer, 0, e.BytesRecorded);*/
        }

        public void BindToBuffer(BufferedWaveProvider outputBuffer)
        {
            if (outputBuffer == null) throw new ArgumentNullException(nameof(outputBuffer), "Buffer cannot be null");

            buffer = outputBuffer;
        }
    }



    /// <summary>
    /// The output device is in a constant state of readiness, meaning it will play any available data immediately.
    /// Control over the audio flow is primarily managed by enabling or disabling the capture device (e.g., starting or stopping a microphone).
    /// </summary>
    public class AudioOutputDevice
    {
        public string name                                      { get; private set; }
        public BufferedWaveProvider bufferForIntercom           { get; private set; }
        public BufferedWaveProvider bufferForSingleAudioPlay    { get; private set; }

        private WasapiOut player                                { get; set; }
        private MMDevice device                                 { get; set; }
        private MixingSampleProvider mixer                      { get; set; }
        private int latencyMs                                   { get; set; } = 10;


        private float _volume;
        /// <summary>
        /// Value must be from 0 to 100
        /// </summary>
        public float volume
        {
            get => _volume;
            set
            {
                if (value >= 0 && value <= 100)
                {
                    _volume = value;
                    player.Volume = volume / 100.0f;
                } else if (value == -1f) return;
                else throw new ArgumentOutOfRangeException("Volume must be a value from 0 to 100");
            }
        }



        public AudioOutputDevice(MMDevice mmDevice, WaveFormat unifiedWaveFormat)
        {
            device                      = mmDevice;
            name                        = device.FriendlyName;

            player                      = new WasapiOut(device, AudioClientShareMode.Shared, false, latencyMs);

            mixer                       = new MixingSampleProvider(unifiedWaveFormat);
            bufferForIntercom           = new BufferedWaveProvider(unifiedWaveFormat);
            bufferForSingleAudioPlay    = new BufferedWaveProvider(unifiedWaveFormat);

            mixer.AddMixerInput(bufferForIntercom);
            mixer.AddMixerInput(bufferForSingleAudioPlay);

            player.Init(mixer);
            player.Play();
        }


        public void SetBufferForSingleAudioPlay(BufferedWaveProvider buffer)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer), "Buffer for single audio play cannot be null");

            mixer.RemoveMixerInput((ISampleProvider)bufferForSingleAudioPlay);
            bufferForSingleAudioPlay = buffer;
            mixer.AddMixerInput(bufferForSingleAudioPlay);

            // Due to the fact that mixer does not automatically understand if you change the link to its buffers,
            // you first have to delete the old one, overwrite it with a new one and only then add the updated buffer
        }

        public void SetBufferForIntercom(BufferedWaveProvider buffer)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer), "Buffer for intercom cannot be null");

            mixer.RemoveMixerInput((ISampleProvider)bufferForIntercom);
            bufferForIntercom = buffer;
            mixer.AddMixerInput(bufferForIntercom);

            // Due to the fact that mixer does not automatically understand if you change the link to its buffers,
            // you first have to delete the old one, overwrite it with a new one and only then add the updated buffer
        }
    }






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
        // Public events
        public event Action? AudioDeviceHasChanged;     // Triggered to update intercom settings when a change in audio devices is detected

        // External links for mapping device names to device objects
        public Dictionary<string, AudioOutputDevice>? audioOutputsDictionary { get; private set; }
        public Dictionary<string, AudioInputDevice>? audioInputsDictionary { get; private set; }

        // Properties representing the selected audio devices for researcher and participant
        public AudioOutputDevice? audioOutputDevice_Researcher { get; private set; }
        public AudioOutputDevice? audioOutputDevice_Participant { get; private set; }
        public AudioInputDevice? audioInputDevice_Researcher { get; private set; }
        public AudioInputDevice? audioInputDevice_Participant { get; private set; }

        /// <summary>
        /// Initializes a new instance of the AudioDevicesParameters class with specified dictionaries mapping device names to their respective objects.
        /// </summary>
        /// <param name="outputsDict">A dictionary mapping output device names to their respective objects.</param>
        /// <param name="inputsDict">A dictionary mapping input device names to their respective objects.</param>
        public AudioDevicesParameters()
        {
            audioOutputsDictionary = null;
            audioInputsDictionary = null;
        }

        public (bool errorOccured, object? extraData) UpdateDevicesDictionaries(Dictionary<string, AudioOutputDevice> outputsDict, Dictionary<string, AudioInputDevice> inputsDict)
        {
            audioOutputsDictionary = outputsDict;
            audioInputsDictionary = inputsDict;


            // Check if any previously selected device has been disconnected (disappeared from the dictionaries)
            // It looks terrible, maybe I'll change it in the future
            //
            // In a nutshell: we go through all the devices and if they are disconnected, we write null.
            // The 'Update' and 'GetAudioData' methods are needed in order not to duplicate the code

            var audioData = GetAudioData();
            var audioDevicesInfo = audioData.audioDevicesInfo;
            if (audioDevicesInfo == null)
                return (errorOccured: true, extraData: audioData);

            foreach (FieldInfo field in typeof(AudioDevicesInfo).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.FieldType == typeof(string))
                {
                    string? value = (string?)field.GetValue(audioDevicesInfo);
                    if (!string.IsNullOrEmpty(value) && !audioOutputsDictionary.ContainsKey(value) && !audioInputsDictionary.ContainsKey(value))
                        field.SetValue(audioDevicesInfo, null);

                }
            }

            var result = Update(audioDevicesInfo);
            return result;
        }


        /// <summary>
        /// Updates the state of audio devices based on the provided parameters. Notifies other components if changes occur.
        /// </summary>
        /// <param name="updatedParameters">New audio device settings to be applied.</param>
        /// <returns>A tuple containing a boolean indicating if an error occurred and an optional object with extra data.</returns>
        public (bool errorOccured, object? extraData) Update(AudioDevicesInfo? updatedParameters)
        {
            if (audioOutputsDictionary == null || audioInputsDictionary == null || updatedParameters == null)
                return (errorOccured: true, extraData: GetAudioData());

            int errorsOccured = 0;
            var changesOccured = 0;

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

                    changesOccured++;
                }
                catch
                {
                    audioOutputDevice_Researcher = null;
                    errorsOccured++;
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

                    changesOccured++;
                }
                catch
                {
                    audioOutputDevice_Participant = null;
                    errorsOccured++;
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

                    changesOccured++;
                }
                catch
                {
                    audioInputDevice_Researcher = null;
                    errorsOccured++;
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

                    changesOccured++;
                }
                catch
                {
                    audioInputDevice_Participant = null;
                    errorsOccured++;
                }
            }

            var UAODVR = updatedParameters.audioOutputDeviceVolume_Researcher;
            if (audioOutputDevice_Researcher != null && audioOutputDevice_Researcher.volume != UAODVR)
            {
                try
                {
                    var value = UAODVR;
                    if (value < 0f || value > 100f || value == null)
                        throw new ArgumentException("audioOutputDeviceVolume_Researcher value is ineligible");

                    audioOutputDevice_Researcher.volume = (float)value;
                }
                catch
                {
                    errorsOccured++;
                }
            }

            var UAODVP = updatedParameters.audioOutputDeviceVolume_Participant;
            if (audioOutputDevice_Participant != null && audioOutputDevice_Participant.volume != UAODVP)
            {
                try
                {
                    var value = UAODVP;
                    if (value < 0f || value > 100f || value == null)
                        throw new ArgumentException("audioOutputDeviceVolume_Participant value is ineligible");

                    audioOutputDevice_Participant.volume = (float)value;
                }
                catch
                {
                    errorsOccured++;
                }
            }

            var UAIDVR = updatedParameters.audioInputDeviceVolume_Researcher;
            if (audioInputDevice_Researcher != null && audioInputDevice_Researcher.volume != UAIDVR)
            {
                try
                {
                    var value = UAIDVR;
                    if (value < 0f || value > 100f || value == null)
                        throw new ArgumentException("audioInputDeviceVolume_Researcher value is ineligible");

                    audioInputDevice_Researcher.volume = (float)value;
                }
                catch
                {
                    errorsOccured++;
                }
            }

            var UAIDVP = updatedParameters.audioInputDeviceVolume_Participant;
            if (audioInputDevice_Participant != null && audioInputDevice_Participant.volume != UAIDVP)
            {
                try
                {
                    var value = UAIDVP;
                    if (value < 0f || value > 100f || value == null)
                        throw new ArgumentException("audioInputDeviceVolume_Participant value is ineligible");

                    audioInputDevice_Participant.volume = (float)value;
                }
                catch
                {
                    errorsOccured++;
                }
            }


            // update Intercoms if at least one device was updated
            if (changesOccured > 0)
                AudioDeviceHasChanged?.Invoke();

            // Report
            if (errorsOccured > 0)
                return (errorOccured: true, extraData: GetAudioData());
            else
                return (errorOccured: false, extraData: null);
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

    }

}
