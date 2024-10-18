#nullable enable

using System;
using System.Collections.Generic;


namespace AudioControl {
    public enum CommandForDaemon
    {
        SetConfigurations,
        PlayAudioClip,
        StartIntercomStream,
        StopIntercomStream,
        Disconnect
    }

    public enum ResponseFromDaemon
    {
        ConfigurationsHaveBeenSet,
        AudioDataHasBeenUpdated,
        ErrorHasOccurred
    }

    /*public class UnifiedAudioDataPacket
    {
    }*/

    public class PlayAudioClip_CommandDetails
    {
        public string audioFileName;
        public string audioOutputDeviceName;

        public PlayAudioClip_CommandDetails(string audioFileName, string audioOutputDeviceName)
        {
            this.audioFileName = audioFileName;
            this.audioOutputDeviceName = audioOutputDeviceName;
        }
    }

    public class SetConfigurations_CommandDetails
    {
        public string unityAudioDirectory;
        // todo: will be more

        public SetConfigurations_CommandDetails(string unityAudioDirectory)
        {
            this.unityAudioDirectory = unityAudioDirectory;
        }
    }

}

#nullable disable