using NAudio.CoreAudioApi;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;

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
        private Dictionary<string, AudioOutputDevice> _audioOutputsDictionary;
        private Dictionary<string, AudioInputDevice> _audioInputsDictionary;

        // Tracks the current configuration of audio devices
        private AudioDevicesInfo _audioDevicesInfo;

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
        public AudioDevicesParameters(Dictionary<string, AudioOutputDevice> outputsDict, Dictionary<string, AudioInputDevice> inputsDict)
        {
            _audioOutputsDictionary = outputsDict;
            _audioInputsDictionary = inputsDict;
            _audioDevicesInfo = new();
        }


        /// <summary>
        /// Updates the state of audio devices based on the provided parameters. Notifies other components if changes occur.
        /// </summary>
        /// <param name="updatedParameters">New audio device settings to be applied.</param>
        /// <returns>A tuple containing a boolean indicating if an error occurred and an optional object with extra data.</returns>
        public (bool errorOccured, object? extraData) Update(AudioDevicesInfo updatedParameters)
        {
            int errorsOccured = 0;
            var audioDeviceHasChanged = false;


            if (this._audioDevicesInfo.audioOutputDeviceName_Researcher != updatedParameters.audioOutputDeviceName_Researcher)
            {
                try
                {
                    if (updatedParameters.audioOutputDeviceName_Researcher == null)
                        audioOutputDevice_Researcher = null;
                    else
                        audioOutputDevice_Researcher = _audioOutputsDictionary[updatedParameters.audioOutputDeviceName_Researcher];

                    audioDeviceHasChanged = true;
                }
                catch
                {
                    audioOutputDevice_Researcher = null;
                    errorsOccured++;
                }
            }

            if (this._audioDevicesInfo.audioOutputDeviceName_Participant != updatedParameters.audioOutputDeviceName_Participant)
            {
                try
                {
                    if (updatedParameters.audioOutputDeviceName_Participant == null)
                        audioOutputDevice_Participant = null;
                    else
                        audioOutputDevice_Participant = _audioOutputsDictionary[updatedParameters.audioOutputDeviceName_Participant];

                    audioDeviceHasChanged = true;
                }
                catch
                {
                    audioOutputDevice_Participant = null;
                    errorsOccured++;
                }
            }

            if (this._audioDevicesInfo.audioInputDeviceName_Researcher != updatedParameters.audioInputDeviceName_Researcher)
            {
                try
                {
                    if (updatedParameters.audioInputDeviceName_Researcher == null)
                        audioInputDevice_Researcher = null;
                    else
                        audioInputDevice_Researcher = _audioInputsDictionary[updatedParameters.audioInputDeviceName_Researcher];
                    AudioDeviceHasChanged?.Invoke();
                }
                catch
                {
                    audioInputDevice_Researcher = null;
                    errorsOccured++;
                }
            }

            if (this._audioDevicesInfo.audioInputDeviceName_Participant != updatedParameters.audioInputDeviceName_Participant)
            {
                try
                {
                    if (updatedParameters.audioInputDeviceName_Participant == null)
                        audioInputDevice_Participant = null;
                    else
                        audioInputDevice_Participant = _audioInputsDictionary[updatedParameters.audioInputDeviceName_Participant];
                    AudioDeviceHasChanged?.Invoke();
                }
                catch
                {
                    audioInputDevice_Participant = null;
                    errorsOccured++;
                }
            }

            if (this._audioDevicesInfo.audioOutputDeviceVolume_Researcher != updatedParameters.audioOutputDeviceVolume_Researcher)
            {
                try
                {
                    var value = updatedParameters.audioOutputDeviceVolume_Researcher;
                    if (value < 0f || value > 100f || value == null)
                        throw new Exception();

                    audioOutputDevice_Researcher.volume = (float)value;
                }
                catch
                {
                    errorsOccured++;
                }
            }

            if (this._audioDevicesInfo.audioOutputDeviceVolume_Participant != updatedParameters.audioOutputDeviceVolume_Participant)
            {
                try
                {
                    var value = updatedParameters.audioOutputDeviceVolume_Participant;
                    if (value < 0f || value > 100f || value == null)
                        throw new Exception();

                    audioOutputDevice_Participant.volume = (float)value;
                }
                catch
                {
                    errorsOccured++;
                }
            }

            // two more IFs for input devices volume change


            // update these data only after IF statements
            _audioDevicesInfo = updatedParameters;


            // update Intercoms if at least one device was updated
            if (audioDeviceHasChanged)
                AudioDeviceHasChanged?.Invoke();

            // Report
            if (errorsOccured > 0)
                return (errorOccured: true, extraData: _audioDevicesInfo);
            else
                return (errorOccured: false, extraData: null);
        }

    }

}
