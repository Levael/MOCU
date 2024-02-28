using CommonUtilitiesNamespace;
using System.Collections.Generic;
#nullable enable


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

    public class UpdateDevicesParameters_Command : GeneralCommandToServer
    {
        public new string Command { get; set; } = "UpdateDevicesParameters_Command";

        public AudioDevicesInfo audioDevicesInfo { get; set; }

        public UpdateDevicesParameters_Command(AudioDevicesInfo audioDevicesInfo)
        {
            this.audioDevicesInfo = audioDevicesInfo;
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

    // Not commands

    public class AudioDevicesInfo
    {
        public string? audioOutputDeviceName_Researcher     { get; set; }
        public string? audioOutputDeviceName_Participant    { get; set; }
        public string? audioInputDeviceName_Researcher      { get; set; }
        public string? audioInputDeviceName_Participant     { get; set; }

        public float? audioOutputDeviceVolume_Researcher    { get; set; }
        public float? audioOutputDeviceVolume_Participant   { get; set; }
        public float? audioInputDeviceVolume_Researcher     { get; set; }
        public float? audioInputDeviceVolume_Participant    { get; set; }

        public AudioDevicesInfo(
            string? audioOutputDeviceNameResearcher = null,
            string? audioInputDeviceNameResearcher = null,
            string? audioOutputDeviceNameParticipant = null,
            string? audioInputDeviceNameParticipant = null,
            float? audioOutputDeviceVolumeResearcher = null,
            float? audioOutputDeviceVolumeParticipant = null,
            float? audioInputDeviceVolumeResearcher = null,
            float? audioInputDeviceVolumeParticipant = null
            )
        {
            audioOutputDeviceName_Researcher = audioOutputDeviceNameResearcher;
            audioOutputDeviceName_Participant = audioOutputDeviceNameParticipant;
            audioInputDeviceName_Researcher = audioInputDeviceNameResearcher;
            audioInputDeviceName_Participant = audioInputDeviceNameParticipant;

            audioOutputDeviceVolume_Researcher = audioOutputDeviceVolumeResearcher;
            audioOutputDeviceVolume_Participant = audioOutputDeviceVolumeParticipant;
            audioInputDeviceVolume_Researcher = audioInputDeviceVolumeResearcher;
            audioInputDeviceVolume_Participant = audioInputDeviceVolumeParticipant;
        }
    }

}
#nullable disable