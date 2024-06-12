using NAudio.CoreAudioApi;
using NAudio.Wave;


namespace AudioControl
{
    public abstract class AudioDevice
    {
        public string name { get; protected set; }
        public MMDevice device { get; protected set; }
        public WaveFormat waveFormat { get; protected set; }

        private float _volume;
        /// <summary>
        /// The value must be between 0 and 100. Any other value will be interpreted as 0 (zero)
        /// </summary>
        public float volume
        {
            get => _volume;
            set
            {
                if (value >= 0 && value <= 100)
                {
                    _volume = value;
                    OnVolumeChanged();
                }
                else
                {
                    _volume = 0;
                    OnVolumeChanged();
                }
            }
        }


        protected AudioDevice(MMDevice mmDevice, WaveFormat unifiedWaveFormat)
        {
            device = mmDevice;
            name = mmDevice.FriendlyName;
            waveFormat = unifiedWaveFormat;
        }


        protected abstract void OnVolumeChanged();
    }
}
