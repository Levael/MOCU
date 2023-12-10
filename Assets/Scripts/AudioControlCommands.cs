namespace AudioControl {

    public class StartIntercomStreamCommand
    {
        public string Command = "StartIntercomStream";
        public int MicrophoneIndex;
        public int SpeakerIndex;
        public string MicrophoneName;
        public string SpeakerName;

        public StartIntercomStreamCommand (int microphoneIndex, int speakerIndex)
        {
            MicrophoneIndex = microphoneIndex;
            SpeakerIndex = speakerIndex;
        }
    }

    public class StopIntercomStreamCommand
    {
        public string Command = "StopIntercomStream";
    }

    public class PlayAudioFileCommand
    {
        public string Command = "PlayAudioFile";
        public string FileName;

        public PlayAudioFileCommand(string fileName)
        {
            FileName = fileName;
        }
    }

    public class GetAudioDevicesCommand
    {
        public string Command = "GetAudioDevices";
        public bool DoUpdate;

        public GetAudioDevicesCommand(bool doUpdate)
        {
            DoUpdate = doUpdate;
        }
    }

}

