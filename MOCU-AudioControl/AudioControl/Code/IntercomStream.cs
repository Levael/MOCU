namespace AudioControl
{
    public class IntercomStream
    {
        private IntercomStreamDirection _direction;
        private bool isStreamOn = false;
        private bool isStreamReady = false;

        private AudioInputDevice? _audioInputDevice;
        private AudioOutputDevice? _audioOutputDevice;


        public IntercomStream(IntercomStreamDirection direction)
        {
            _direction = direction;
            isStreamReady = false;
        }

        public void StartStream()
        {
            if (!isStreamReady || isStreamOn) return;
            if (_audioInputDevice == null)
            {
                isStreamOn = false;
                return;
            }

            isStreamOn = _audioInputDevice.StartRecording();    // "true" if started correctly
        }

        public void StopStream()
        {
            if (!isStreamOn) return;
            if (_audioInputDevice == null)
            {
                isStreamOn = false;
                return;
            }

            isStreamOn = !(_audioInputDevice.StopRecording());  // "false" if stoped correctly
        }


        public void UpdateDevices(AudioInputDevice? audioInputDevice, AudioOutputDevice? audioOutputDevice)
        {
            if (isStreamOn) StopStream();

            _audioInputDevice = audioInputDevice;
            _audioOutputDevice = audioOutputDevice;

            if (_audioInputDevice == null || _audioOutputDevice == null)
            {
                isStreamOn = false;
                isStreamReady = false;
                return;
            }

            BindInputToOutput();
            isStreamReady = true;
        }

        private void BindInputToOutput()
        {
            try { _audioInputDevice.BindToBuffer(_audioOutputDevice.bufferForIntercom); } catch { }
        }
    }



    public enum IntercomStreamDirection
    {
        Incoming,   // from Participant to Researcher
        Outgoing    // from Researcher to Participant
    }
}
