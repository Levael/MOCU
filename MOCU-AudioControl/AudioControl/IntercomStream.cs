using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace AudioControl
{
    public class IntercomStream
    {
        private IntercomStreamDirection _direction;
        private bool isStreamOn = false;
        private bool isStreamReady = false;

        public string  audioInputDeviceName;
        public string  audioOutputDeviceName;
        public float   audioOutputDeviceVolume;

        private AudioInputDevice _audioInputDevice;
        private AudioOutputDevice _audioOutputDevice;

        private AudioManager _audioManager;

        public IntercomStream(IntercomStreamDirection direction, AudioManager audioManager, string audioInputDeviceName = "", string audioOutputDeviceName = "", float audioOutputDeviceVolume = -1f)
        {
            _direction = direction;
            _audioManager = audioManager;
            this.audioOutputDeviceVolume = 50f; // default value

            if (string.IsNullOrEmpty(audioInputDeviceName) || string.IsNullOrEmpty(audioOutputDeviceName))
            {
                isStreamReady = false;
                return;
            }

            UpdateAudioDevices(audioInputDeviceName: audioInputDeviceName, audioOutputDeviceName: audioOutputDeviceName, audioOutputDeviceVolume: audioOutputDeviceVolume);

            isStreamReady = true;
        }

        public void StartStream()
        {
            if (!isStreamReady || isStreamOn) return;

            isStreamOn = _audioInputDevice.StartRecording();    // "true" if started correctly
        }

        public void StopStream()
        {
            if (!isStreamOn) return;

            isStreamOn = !(_audioInputDevice.StopRecording());  // "false" if stoped correctly
        }

        /// <summary>
        /// Sets or reloads the input device based on the provided device name
        /// </summary>
        private void SetInputDevice(string audioInputDeviceName)
        {
            // if function didn't get any parameter -- just reload the object (looks like this because of error CS1736)
            this.audioInputDeviceName = audioInputDeviceName == "same" ? this.audioInputDeviceName : audioInputDeviceName;

            if (string.IsNullOrEmpty(this.audioInputDeviceName)) throw new InvalidOperationException("Device name can't be Null or Empty");
            if (isStreamOn) StopStream();

            _audioInputDevice = _audioManager.audioInputsDictionary[this.audioInputDeviceName];
        }

        /// <summary>
        /// Sets or reloads the output device based on the provided device name
        /// </summary>
        private void SetOutputDevice(string audioOutputDeviceName, float audioOutputDeviceVolume)
        {
            // if function didn't get any parameter -- just same the object (looks like this because of error CS1736)
            this.audioOutputDeviceName = audioOutputDeviceName == "same" ? this.audioOutputDeviceName : audioOutputDeviceName;
            this.audioOutputDeviceVolume = audioOutputDeviceVolume == -1f ? this.audioOutputDeviceVolume : audioOutputDeviceVolume;

            if (string.IsNullOrEmpty(audioOutputDeviceName)) throw new InvalidOperationException("Device name can't be Null or Empty");
            if (isStreamOn) StopStream();

            _audioOutputDevice = _audioManager.audioOutputsDictionary[this.audioOutputDeviceName];
            _audioOutputDevice.volume = this.audioOutputDeviceVolume;
        }

        /// <summary>
        /// When passing a value to a function, it is necessary to indicate the specific type of device
        /// </summary>
        public void UpdateAudioDevices(string audioInputDeviceName, string audioOutputDeviceName, float audioOutputDeviceVolume)
        {
            if (audioOutputDeviceVolume != -1f) this.audioOutputDeviceVolume = audioOutputDeviceVolume; // must be before "SetOutputDevice"

            SetInputDevice(audioInputDeviceName);
            SetOutputDevice(audioOutputDeviceName, audioOutputDeviceVolume);
            BindInputToOutput(audioInputDeviceName, audioOutputDeviceName);

            isStreamReady = true;
        }

        private void BindInputToOutput(string inputDeviceName, string outputDeviceName)
        {
            _audioInputDevice.BindToBuffer(_audioOutputDevice.bufferForIntercom);
        }
    }




    public enum IntercomStreamDirection
    {
        Incoming,   // from Participant to Researcher
        Outgoing    // from Researcher to Participant
    }
}
