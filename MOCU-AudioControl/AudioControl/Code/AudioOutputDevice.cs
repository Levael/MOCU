using System.Linq;
using NAudio.CoreAudioApi;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using NAudio.Mixer;


namespace AudioModule.Daemon
{

    public class AudioOutputDevice
    {
        private WasapiOut _player { get; set; }
        private MixingSampleProvider _mixer { get; set; }
        private WaveFormat _format { get; set; }
        private int _bufferSize { get; set; }


        public AudioOutputDevice(MMDevice device)
        {
            _format = UnifiedAudioFormat.WaveFormat;
            _bufferSize = UnifiedAudioFormat.BufferSize;

            _mixer = new MixingSampleProvider(_format);
            _mixer.AddMixerInput(new SilenceProvider(_format));

            _player = new WasapiOut(device, AudioClientShareMode.Shared, true, _bufferSize);
            _player.Init(_mixer);
            

            // Does not work in vain until there is no input data (can't pause nothing, so 'play' at first, and only then 'pause')
            // TODO: maybe delete those 2 line later (may be redundant)
            /*_player.Play();
            _player.Pause();*/
        }


        public void AddSampleProvider(ISampleProvider buffer)
        {
            _mixer.AddMixerInput(buffer);

            if (_mixer.MixerInputs.Count() > 0 && _player.PlaybackState != PlaybackState.Playing)
                _player.Play();
        }

        public void RemoveSampleProvider(ISampleProvider buffer)
        {
            _mixer.RemoveMixerInput(buffer);

            if (_mixer.MixerInputs.Count() == 0 && _player.PlaybackState == PlaybackState.Playing)
                _player.Pause();
        }
    }


}