using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Collections.Generic;

// todo: clear all buffers if device was disconected

namespace AudioModule.Daemon
{
    public class AudioInputDevice : IAudioDevice, IDisposable
    {
        private readonly MMDevice _device;
        private WasapiCapture _receiver;
        private readonly List<BufferedWaveProvider> _buffers;
        private readonly object _buffersLock;

        public Guid Id { get; private set; }


        public AudioInputDevice(MMDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            _device = device;
            _buffersLock = new();
            _buffers = new();

            Id = Utils.ExtractGuid(_device.ID);

            _receiver = new WasapiCapture(device, UnifiedAudioFormat.UseEventSync, UnifiedAudioFormat.BufferSize);
            _receiver.WaveFormat = UnifiedAudioFormat.WaveFormat;
            _receiver.DataAvailable += OnDataAvailable;

            //Console.WriteLine($"Created new 'AudioInputDevice'.\nID: {_device.ID}\nName: {_device.FriendlyName}");
        }

        public void Reinitialize()
        {
            if (_receiver != null)
            {
                _receiver.DataAvailable -= OnDataAvailable;
                _receiver.StopRecording();
                _receiver.Dispose();
            }

            _receiver = new WasapiCapture(_device, UnifiedAudioFormat.UseEventSync, UnifiedAudioFormat.BufferSize);
            _receiver.WaveFormat = UnifiedAudioFormat.WaveFormat;
            _receiver.DataAvailable += OnDataAvailable;

            if (_buffers.Count > 0 && _receiver.CaptureState != CaptureState.Capturing)
                _receiver.StartRecording();
        }

        /// <summary>
        /// It is recommended to keep it within 70. +-10
        /// </summary>
        public float Volume
        {
            get => _device.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
            set => _device.AudioEndpointVolume.MasterVolumeLevelScalar = Math.Clamp(value / 100, 0f, 1f);
        }

        public void AddBinding(BufferedWaveProvider buffer)
        {
            lock (_buffersLock)
                _buffers.Add(buffer);

            if (_receiver.CaptureState != CaptureState.Capturing)
                _receiver.StartRecording();
        }

        public void RemoveBinding(BufferedWaveProvider buffer)
        {
            lock (_buffersLock)
                _buffers.Remove(buffer);

            if (_buffers.Count == 0)
                _receiver.StopRecording();
        }

        // todo: looks a bit messy, maybe reactor later
        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            // First 'try-catch' is for "deleting while 'foreach'" errors
            // Second 'try-catch' is for error whith specific buffer

            try
            {
                foreach (var buffer in _buffers)
                {
                    try
                    {
                        buffer.AddSamples(e.Buffer, 0, e.BytesRecorded);    // todo: maybe consider making it in separate thread
                    }
                    catch (Exception ex)
                    {
                        RemoveBinding(buffer);
                        Console.WriteLine($"AudioInputDevice.OnDataAvailable - buffer removed due to an error while trying to 'AddSamples'. Error: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Will just ignore on sample of data
                Console.WriteLine($"AudioInputDevice.OnDataAvailable - an error occurred. Most likely because of deleting while 'foreach'. Error: {ex}");
            }  
        }

        public void Dispose()
        {
            if (_receiver != null)
            {
                _receiver.DataAvailable -= OnDataAvailable;
                _receiver.StopRecording();
                _receiver.Dispose();
            }

            lock (_buffersLock)
                _buffers.Clear();
        }
    }
}