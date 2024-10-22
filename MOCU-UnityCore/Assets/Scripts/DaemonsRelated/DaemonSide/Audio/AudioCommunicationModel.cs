using System;
using System.Collections.Generic;


namespace AudioModule {

    public enum ResponseFromDaemon
    {
        ParametersHaveChanged,
        ErrorOccurred
    }

    public class PlayAudioClip_MessageDetails
    {
        public AudioClipData clipData { get; set; }
        public Guid outputDeviceId { get; set; }
    }

    public class UpdateParameters_MessageDetails
    {
        public IEnumerable<AudioClipData> audioClipChanges { get; set; }
        public IEnumerable<AudioDeviceData> audiodeviceChanges { get; set; }
    }

    public class DaemonErrorReport
    {
        public string message { get; set; }
        public bool isFatal { get; set; }
        public object payload { get; set; }
    }
}