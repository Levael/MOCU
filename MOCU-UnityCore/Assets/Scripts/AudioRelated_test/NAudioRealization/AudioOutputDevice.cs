using System.Linq;
using NAudio.CoreAudioApi;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;

namespace AudioModule_NAudio
{
    
    public class AudioOutputDevice
    {
        private WasapiOut _player { get; set; }
        private MixingSampleProvider _mixer { get; set; }

        private BufferedWaveProvider _bufferedWaveProvider { get; set; }
        private WaveFormat _format {  get; set; }


        public AudioOutputDevice(MMDevice device)
        {
            
            _format = WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);


            _player = new WasapiOut(device, AudioClientShareMode.Shared, true, UnifiedAudioFormat.BufferSize);
            
            //_mixer = new MixingSampleProvider(new WaveFormat(44100, 16, 2));
            _bufferedWaveProvider = new BufferedWaveProvider(_format);

            WaveFormatExtensible closestSupportedFormat = null;
            //if (!device.AudioClient.IsFormatSupported(AudioClientShareMode.Shared, _format, out closestSupportedFormat))
                //UnityEngine.Debug.Log($"{device.AudioClient.BufferSize}, {device.State}, {device.FriendlyName}, {closestSupportedFormat}");

            _player.Init(_bufferedWaveProvider);
            //_player.Init(_mixer);

            // Does not work in vain until there is no input data (can't pause nothing, so 'play' at first, and only then 'pause')
            // TODO: maybe delete those 2 line later (may be redundant)
            _player.Play();
            _player.Pause();
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
