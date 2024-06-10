#nullable enable
#pragma warning disable CS8618
using System.Collections.Generic;

using DaemonsNamespace.InterprocessCommunication;
using Newtonsoft.Json;




namespace AudioControl {
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

    // extraData  -------------------------------------------------------------------------------------------------
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

    // Not commands

    /// <summary>
    /// Class for storing audio device data, also used for transferring device information between server and client
    /// </summary>
    public class AudioDevicesInfo
    {
        public string?  audioOutputDeviceName_Researcher;
        public string?  audioOutputDeviceName_Participant;
        public string?  audioInputDeviceName_Researcher;
        public string?  audioInputDeviceName_Participant;

        public float?   audioOutputDeviceVolume_Researcher  = 50;
        public float?   audioOutputDeviceVolume_Participant = 50;
        public float?   audioInputDeviceVolume_Researcher   = 70;
        public float?   audioInputDeviceVolume_Participant  = 70;
    }

    // todo: change later List to Enumerable
    public class AudioDevicesLists
    {
        public List<string>? InputDevices;
        public List<string>? OutputDevices;
    }

    public class UnifiedAudioDataPacket
    {
        public readonly AudioDevicesInfo? audioDevicesInfo;
        public readonly List<string>? inputAudioDevices;
        public readonly List<string>? outputAudioDevices;

        public UnifiedAudioDataPacket(AudioDevicesInfo? audioDevicesInfo, List<string>? inputAudioDevices, List<string>? outputAudioDevices)
        {
            this.audioDevicesInfo = audioDevicesInfo;
            this.inputAudioDevices = inputAudioDevices;
            this.outputAudioDevices = outputAudioDevices;
        }
    }

}

#nullable disable
#pragma warning restore CS8618