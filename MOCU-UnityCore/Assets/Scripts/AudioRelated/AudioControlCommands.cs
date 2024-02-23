using CommonUtilitiesNamespace;
using System.Collections.Generic;

namespace AudioControl {
    // COMMON ====================================================================================================

    public abstract class GeneralResponseFromServer
    {
        public string Command           { get; set; }
        public string ReceivedCommand   { get; set; }
        public bool HasError            { get; set; }
    }

    public abstract class GeneralCommandToServer
    {
        public string Command           { get; set; }
    }

    // RESPONSES =================================================================================================

    public class GeneralResponseFromServer_Command : GeneralResponseFromServer
    {
        public new string Command           { get; set; } = "GeneralResponseFromServer_Command";
        public new string ReceivedCommand   { get; set; }
        public new bool HasError            { get; set; }

        public GeneralResponseFromServer_Command(string receivedCommand, bool hasError)
        {
            ReceivedCommand = receivedCommand;
            HasError = hasError;
        }
    }

    public class ResponseFromServer_GetAudioDevices_Command : GeneralResponseFromServer
    {
        public new string Command           { get; set; } = "ResponseFromServer_GetAudioDevices_Command";
        public new string ReceivedCommand   { get; set; } = "GetAudioDevices_Command";
        public new bool HasError            { get; set; } = false;

        public List<string> InputDevices    { get; set; }
        public List<string> OutputDevices   { get; set; }

        public ResponseFromServer_GetAudioDevices_Command(List<string> inputDevices, List<string> outputDevices)
        {
            InputDevices = inputDevices;
            OutputDevices = outputDevices;
        }
    }

    // COMMANDS ==================================================================================================
    // intercom --------------------------------------------------------------------------------------------------

    public class StartIntercomStream_ResearcherToParticipant_Command : GeneralCommandToServer
    {
        public new string Command = "StartIntercomStream_ResearcherToParticipant_Command";
    }
    
    public class StartIntercomStream_ParticipantToResearcher_Command : GeneralCommandToServer
    {
        public new string Command { get; set; } = "StartIntercomStream_ParticipantToResearcher_Command";
    }

    public class StopIntercomStream_ResearcherToParticipant_Command : GeneralCommandToServer
    {
        public new string Command { get; set; } = "StopIntercomStream_ResearcherToParticipant_Command";
    }
    
    public class StopIntercomStream_ParticipantToResearcher_Command : GeneralCommandToServer
    {
        public new string Command { get; set; } = "StopIntercomStream_ParticipantToResearcher_Command";
    }

    // different -------------------------------------------------------------------------------------------------
    public class PlayAudioFile_Command : GeneralCommandToServer
    {
        public new string Command           { get; set; } = "PlayAudioFile_Command";

        public string AudioFileName         { get; set; }
        public string AudioOutputDeviceName { get; set; }

        public PlayAudioFile_Command(string audioFileName, string audioOutputDeviceName)
        {
            AudioFileName = audioFileName;
            AudioOutputDeviceName = audioOutputDeviceName;
        }
    }

    public class SendConfigs_Command : GeneralCommandToServer
    {
        public new string Command           { get; set; } = "SendConfigs_Command";

        public string UnityAudioDirectory   { get; set; }

        public SendConfigs_Command(string unityAudioDirectory)
        {
            UnityAudioDirectory = unityAudioDirectory;
        }
    }

    public class GetAudioDevices_Command : GeneralCommandToServer
    {
        public new string Command   { get; set; } = "GetAudioDevices_Command";

        public bool DoUpdate        { get; set; }
        // The server stores the list requested once, which means the response will be fast.
        // If it is known that the devices have changed, the flag must be set to true

        public GetAudioDevices_Command(bool doUpdate)
        {
            DoUpdate = doUpdate;
        }
    }

    public class SetDevicesParameters_Command : GeneralCommandToServer
    {
        public new string Command                       { get; set; } = "SetDevicesParameters_Command";

        public string AudioOutputDeviceNameResearcher   { get; set; }
        public string AudioInputDeviceNameResearcher    { get; set; }
        public string AudioOutputDeviceNameParticipant  { get; set; }
        public string AudioInputDeviceNameParticipant   { get; set; }

        public float AudioOutputDeviceVolumeResearcher  { get; set; }
        public float AudioOutputDeviceVolumeParticipant { get; set; }

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

    public class ChangeOutputDeviceVolume_Command : GeneralCommandToServer
    {
        public new string Command   { get; set; } = "ChangeOutputDeviceVolume_Command";
        public string Name          { get; set; }
        public float Volume         { get; set; }

        public ChangeOutputDeviceVolume_Command( string name, float volume )
        {
            Name = name;
            Volume = volume;
        }
    }

    public class AudioDevicesInfo
    {
        // Device names that don't affect anything outside the app
        public string audioOutputDeviceName_Researcher { get; set; }
        public string audioOutputDeviceName_Participant { get; set; }
        public string audioInputDeviceName_Researcher { get; set; }
        public string audioInputDeviceName_Participant { get; set; }

        // Volume levels adjusted according to a specific protocol
        public float currentAudioOutputDeviceVolume_Researcher { get; set; }
        public float currentAudioOutputDeviceVolume_Participant { get; set; }
        public float currentAudioInputDeviceVolume_Researcher { get; set; }
        public float currentAudioInputDeviceVolume_Participant { get; set; }

        // Volume levels before the program was initiated
        public float previousAudioOutputDeviceVolume_Researcher { get; set; }
        public float previousAudioOutputDeviceVolume_Participant { get; set; }
        public float previousAudioInputDeviceVolume_Researcher { get; set; }
        public float previousAudioInputDeviceVolume_Participant { get; set; }
    }

}

