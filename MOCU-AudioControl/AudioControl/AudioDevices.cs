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
}
