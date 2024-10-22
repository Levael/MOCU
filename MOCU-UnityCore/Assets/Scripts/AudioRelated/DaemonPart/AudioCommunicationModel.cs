using System;
using System.Collections.Generic;


namespace AudioModule
{
    // ============================================================================================
    // DTOs =======================================================================================
    // ============================================================================================

    public class DataTrasferObjectToAudioDaemon
    {
        public MessageNameToAudioDaemon messageName { get; set; }
        public object data { get; set; }
    }

    public class DataTrasferObjectFromAudioDaemon
    {
        public MessageNameFromAudioDaemon messageName { get; set; }
        public object data { get; set; }
    }

    // ============================================================================================
    // enum classes ===============================================================================
    // ============================================================================================

    public enum MessageNameToAudioDaemon
    {
        PlayAudioClip,
        UpdateParameters,
        StartIntercom,
        StopIntercom,

        Disconnect
    }

    public enum MessageNameFromAudioDaemon
    {
        ParametersHaveChanged,
        ErrorOccurred
    }

    // ============================================================================================
    // MessageToAudioDaemon - data classes ========================================================
    // ============================================================================================

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

    // ============================================================================================
    // MessageFromAudioDaemon - data classes ======================================================
    // ============================================================================================

    public class DaemonErrorReport
    {
        public string message { get; set; }
        public bool isFatal { get; set; }
        public object payload { get; set; }
    }
}