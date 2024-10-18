using NAudio.CoreAudioApi;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;


namespace AudioControl
{
    /// <summary>
    /// The output device is in a constant state of readiness, meaning it will play any available data immediately.
    /// Control over the audio flow is primarily managed by enabling or disabling the capture device (e.g., starting or stopping a microphone).
    /// </summary>
    public class AudioOutputDevice : AudioDevice
    {
        public BufferedWaveProvider bufferForIntercom { get; private set; }
        public BufferedWaveProvider bufferForSingleAudioPlay { get; private set; }

        private WasapiOut player { get; set; }
        private MixingSampleProvider mixer { get; set; }
        private int latencyMs { get; set; } = 10;



        public AudioOutputDevice(MMDevice mmDevice, WaveFormat unifiedWaveFormat) : base(mmDevice, unifiedWaveFormat)
        {
            player = new WasapiOut(device, AudioClientShareMode.Shared, true, latencyMs);

            mixer = new MixingSampleProvider(waveFormat);
            bufferForIntercom = new BufferedWaveProvider(waveFormat);
            bufferForSingleAudioPlay = new BufferedWaveProvider(waveFormat);

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

        protected override void OnVolumeChanged()
        {
            player.Volume = volume / 100.0f;
        }
    }
}
