﻿using NAudio.CoreAudioApi;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;


namespace AudioModule.Daemon
{
    public class AudioOutputDevice : IAudioDevice, IDisposable
    {
        private readonly MMDevice _device;
        private WasapiOut _player { get; set; }
        private MixingSampleProvider _mixer { get; set; }

        public AudioOutputDevice(MMDevice device)
        {
            _device = device;

            _mixer = new MixingSampleProvider(UnifiedAudioFormat.WaveFormat);
            _mixer.MixerInputEnded += (sender, args) => SampleProviderHasBeenRemoved();

            _player = new WasapiOut(_device, AudioClientShareMode.Shared, UnifiedAudioFormat.UseEventSync, UnifiedAudioFormat.BufferSize);
            _player.Init(_mixer);


            Console.WriteLine($"Created new 'AudioOutputDevice' with ID: {_device.ID}");
        }

        public float Volume
        {
            get => _player.Volume * 100;
            set => _player.Volume = Math.Clamp(value / 100, 0f, 1f);
        }

        public IEnumerable<ISampleProvider> GetSampleProviders()
        {
            return _mixer.MixerInputs;
        }

        public void AddSampleProvider(ISampleProvider buffer)
        {
            _mixer.AddMixerInput(buffer);
            SampleProviderHasBeenAdded();
        }

        public void AddSampleProvider(IWaveProvider buffer)
        {
            _mixer.AddMixerInput(buffer);
            SampleProviderHasBeenAdded();
        }

        public void RemoveSampleProvider(ISampleProvider buffer)
        {
            _mixer.RemoveMixerInput(buffer);
            SampleProviderHasBeenRemoved();
        }

        private void SampleProviderHasBeenAdded()
        {
            if (_mixer.MixerInputs.Any() && _player.PlaybackState != PlaybackState.Playing)
                _player.Play();
        }

        private void SampleProviderHasBeenRemoved()
        {
            // If there are lags or smth similar, try to not 'Pause' when there is no inputs.
            // It will run idle but without switching delays.
            // This 'Pause' is only to save the power of the audio device and processor to play silence.

            if (!_mixer.MixerInputs.Any() && _player.PlaybackState == PlaybackState.Playing)
                _player.Pause();
        }

        public void Dispose()
        {
            _player?.Stop();
            _player?.Dispose();
        }
    }
}