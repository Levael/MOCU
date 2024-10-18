using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Linq;


namespace AudioModule_NAudio
{
    
    public class AudioInputDevice
    {
        private WasapiCapture _receiver { get; set; }
        private EventHandler<WaveInEventArgs> _dataAvailable;


        public AudioInputDevice(MMDevice device)
        {
            _receiver = new WasapiCapture(device, true, UnifiedAudioFormat.BufferSize);
            _receiver.WaveFormat = UnifiedAudioFormat.WaveFormat;

            _receiver.DataAvailable += OnDataAvailableInternal;
        }


        public event EventHandler<WaveInEventArgs> DataAvailable
        {
            add
            {
                if (_dataAvailable == null || !_dataAvailable.GetInvocationList().Contains(value))
                {
                    _dataAvailable += value;

                    if (_receiver.CaptureState != CaptureState.Capturing)
                        _receiver.StartRecording();
                }
            }
            remove
            {
                _dataAvailable -= value;

                if ((_dataAvailable == null || _dataAvailable.GetInvocationList().Length == 0) && _receiver.CaptureState != CaptureState.Stopped)
                    _receiver.StopRecording();
            }
        }


        private void OnDataAvailableInternal(object sender, WaveInEventArgs e)
        {
            _dataAvailable?.Invoke(this, e);
        }
    }

}
