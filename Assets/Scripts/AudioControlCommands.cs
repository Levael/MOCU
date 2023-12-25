namespace AudioControl {

    // INTERCOM ==================================================================================================
    public class StartIntercomStream_ResearcherToParticipant_Command
    {
        public string Command = "StartIntercomStream_ResearcherToParticipant_Command";
    }
    
    public class StartIntercomStream_ParticipantToResearcher_Command
    {
        public string Command = "StartIntercomStream_ParticipantToResearcher_Command";
    }

    public class StopIntercomStream_ResearcherToParticipant_Command
    {
        public string Command = "StopIntercomStream_ResearcherToParticipant_Command";
    }
    
    public class StopIntercomStream_ParticipantToResearcher_Command
    {
        public string Command = "StopIntercomStream_ParticipantToResearcher_Command";
    }

    // DIFFERENT =================================================================================================
    public class PlayAudioFile_Command
    {
        public string Command = "PlayAudioFile_Command";

        public string AudioFileName;
        public string AudioOutputDeviceName;

        public PlayAudioFile_Command(string audioFileName, string audioOutputDeviceName)
        {
            AudioFileName = audioFileName;
            AudioOutputDeviceName = audioOutputDeviceName;
        }
    }

    public class GetAudioDevices_Command
    {
        public string Command = "GetAudioDevices_Command";

        public bool DoUpdate;
        // The server stores the list requested once, which means the response will be fast.
        // If it is known that the devices have changed, the flag must be set to true

        public GetAudioDevices_Command(bool doUpdate)
        {
            DoUpdate = doUpdate;
        }
    }

    public class SetDevicesParameters_Command
    {
        public string Command = "SetDevicesParameters_Command";

        public string AudioOutputDeviceNameResearcher;
        public string AudioInputDeviceNameResearcher;
        public string AudioOutputDeviceNameParticipant;
        public string AudioInputDeviceNameParticipant;

        public float AudioOutputDeviceVolumeResearcher;
        public float AudioOutputDeviceVolumeParticipant;

        public SetDevicesParameters_Command(
            string audioOutputDeviceNameResearcher = "same",
            string audioInputDeviceNameResearcher = "same",
            string audioOutputDeviceNameParticipant = "same",
            string audioInputDeviceNameParticipant = "same",
            float audioOutputDeviceVolumeResearcher = -1f,
            float audioOutputDeviceVolumeParticipant = -1f
            )
        {
            AudioOutputDeviceNameResearcher = audioOutputDeviceNameResearcher;
            AudioInputDeviceNameResearcher = audioInputDeviceNameResearcher;
            AudioOutputDeviceNameParticipant = audioOutputDeviceNameParticipant;
            AudioInputDeviceNameParticipant = audioInputDeviceNameParticipant;
            AudioOutputDeviceVolumeResearcher = audioOutputDeviceVolumeResearcher;
            AudioOutputDeviceVolumeParticipant = audioOutputDeviceVolumeParticipant;
        }
    }

    public class ChangeOutputDeviceVolume_Command
    {
        public float AudioOutputDeviceVolumeResearcher;
        public float AudioOutputDeviceVolumeParticipant;

        public ChangeOutputDeviceVolume_Command(
            float audioOutputDeviceVolumeResearcher = -1f,
            float audioOutputDeviceVolumeParticipant = -1f
            )
        {
            AudioOutputDeviceVolumeResearcher = audioOutputDeviceVolumeResearcher;
            AudioOutputDeviceVolumeParticipant = audioOutputDeviceVolumeParticipant;
        }
    }

}

