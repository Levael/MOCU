using NAudio.CoreAudioApi;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
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


    
    /*/// <summary>
    /// Holds the configuration and state of audio devices used within the application. 
    /// This includes both input and output devices for researchers and participants. 
    /// It tracks current and previous volume levels to ensure proper audio management 
    /// and restoration upon application closure. This class ensures that changes made 
    /// within the application do not affect external system settings permanently.
    /// </summary>*/
    public class AudioDevicesParameters
    {
        // External links
        private Dictionary<string, AudioOutputDevice> _audioOutputsDictionary;
        private Dictionary<string, AudioInputDevice> _audioInputsDictionary;

        // Event
        public event Action<bool>? SendResponseToUnity; // call only when error uccures
        public event Action? OutputDeviceHasChanged;
        public event Action? InputDeviceHasChanged;

        // Private fields
        private string? _audioOutputDeviceName_Researcher;
        private string? _audioOutputDeviceName_Participant;
        private string? _audioInputDeviceName_Researcher;
        private string? _audioInputDeviceName_Participant;

        private float? _audioOutputDeviceVolume_Researcher;
        private float? _audioOutputDeviceVolume_Participant;
        private float? _audioInputDeviceVolume_Researcher;
        private float? _audioInputDeviceVolume_Participant;

        private AudioOutputDevice? _audioOutputDevice_Researcher;
        private AudioOutputDevice? _audioOutputDevice_Participant;
        private AudioInputDevice? _audioInputDevice_Researcher;
        private AudioInputDevice? _audioInputDevice_Participant;


        public AudioDevicesParameters(Dictionary<string, AudioOutputDevice> outputsDict, Dictionary<string, AudioInputDevice> inputsDict)
        {
            _audioOutputsDictionary = outputsDict;
            _audioInputsDictionary = inputsDict;
        }


        // Public properties
        public string? audioOutputDeviceName_Researcher
        {
            get => _audioOutputDeviceName_Researcher;
            set
            {
                try
                {
                    _audioOutputDeviceName_Researcher = value;
                    _audioOutputDevice_Researcher = _audioOutputsDictionary[value];
                    OutputDeviceHasChanged?.Invoke();
                }
                catch
                {
                    _audioOutputDeviceName_Researcher = null;
                    _audioOutputDevice_Researcher = null;
                    SendResponseToUnity?.Invoke(false);
                }
            }
        }

        public string? audioOutputDeviceName_Participant
        {
            get => _audioOutputDeviceName_Participant;
            set
            {
                try
                {
                    _audioOutputDeviceName_Participant = value;
                    _audioOutputDevice_Participant = _audioOutputsDictionary[value];
                    OutputDeviceHasChanged?.Invoke();
                }
                catch
                {
                    _audioOutputDeviceName_Participant = null;
                    _audioOutputDevice_Participant = null;
                    SendResponseToUnity?.Invoke(false);
                }
            }
        }

        public string? audioInputDeviceName_Researcher
        {
            get => _audioInputDeviceName_Researcher;
            set
            {
                try
                {
                    _audioInputDeviceName_Researcher = value;
                    _audioInputDevice_Researcher = _audioInputsDictionary[value];
                    InputDeviceHasChanged?.Invoke();
                }
                catch
                {
                    _audioInputDeviceName_Researcher = null;
                    _audioInputDevice_Researcher = null;
                    SendResponseToUnity?.Invoke(false);
                }
            }
        }

        public string? audioInputDeviceName_Participant
        {
            get => _audioInputDeviceName_Participant;
            set
            {
                try
                {
                    _audioInputDeviceName_Participant = value;
                    _audioInputDevice_Participant = _audioInputsDictionary[value];
                    InputDeviceHasChanged?.Invoke();
                }
                catch
                {
                    _audioInputDeviceName_Participant = null;
                    _audioInputDevice_Participant = null;
                    SendResponseToUnity?.Invoke(false);
                }
            }
        }

        public float? audioOutputDeviceVolume_Researcher
        {
            get => _audioOutputDeviceVolume_Researcher;
            set
            {
                try
                {
                    if (value < 0f || value > 100f || value == null)
                        throw new Exception();

                    _audioOutputDeviceVolume_Researcher = value;
                    _audioOutputDevice_Researcher.volume = (float)value;
                }
                catch
                {
                    SendResponseToUnity?.Invoke(false);
                }
            }
        }

        public float? audioOutputDeviceVolume_Participant
        {
            get => _audioOutputDeviceVolume_Participant;
            set
            {
                try
                {
                    if (value < 0f || value > 100f || value == null)
                        throw new Exception();

                    _audioOutputDeviceVolume_Participant = value;
                    _audioOutputDevice_Participant.volume = (float)value;
                }
                catch
                {
                    SendResponseToUnity?.Invoke(false);
                }
            }
        }

        public float? audioInputDeviceVolume_Researcher
        {
            // currently with no use
            get => _audioInputDeviceVolume_Researcher;
            set
            {
                _audioInputDeviceVolume_Researcher = value;
            }
        }

        public float? audioInputDeviceVolume_Participant
        {
            // currently with no use
            get => _audioInputDeviceVolume_Participant;
            set
            {
                _audioInputDeviceVolume_Participant = value;
            }
        }

        public AudioOutputDevice? audioOutputDevice_Researcher { get; private set; }
        public AudioOutputDevice? audioOutputtDevice_Participant { get; private set; }
        public AudioInputDevice? audioInputDevice_Researcher { get; private set; }
        public AudioInputDevice? audioInputDevice_Participant { get; private set; }




        /// <summary>
        /// Updates properties of the current instance with values from another instance if they are different.
        /// </summary>
        /// <param name="updatedParams">The instance of AudioDevicesParameters with updated values (got from client JSON)</param>
        /// <returns>Return status of operation. True if everything is ok, and false if any error uccurred</returns>
        public bool Update(AudioDevicesParameters updatedParameters)
        {
            try
            {
                foreach (PropertyInfo property in typeof(AudioDevicesParameters).GetProperties())
                {
                    var currentValue = property.GetValue(this);     // 'this' == current 'AudioDevicesParameters' class
                    var newValue = property.GetValue(updatedParameters);

                    if (!Equals(currentValue, newValue))
                    {
                        property.SetValue(this, newValue);
                    }
                }

                return true;
            }
            catch { return false; }
        }
    }




}
