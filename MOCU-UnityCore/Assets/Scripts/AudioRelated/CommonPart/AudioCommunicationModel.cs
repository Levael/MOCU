using System;
using System.Collections.Generic;
using DaemonsRelated;


namespace AudioModule
{
    public class AudioDataTransferObject
    {
        public IEnumerable<AudioClipData> ClipChanges { get; set; }
        public IEnumerable<AudioDeviceData> DeviceChanges { get; set; }
        public IEnumerable<DaemonErrorReport> DaemonErrorReports { get; set; }
        public IEnumerable<AudioIntercomData> IntercomCommands { get; set; }
        public IEnumerable<PlayAudioClipCommand> PlayClipCommands { get; set; }
        public bool DoTerminateTheDaemon { get; set; }
    }

    public class PlayAudioClipCommand
    {
        public AudioClipData ClipData { get; set; }
        public Guid OutputDeviceId { get; set; }
    }
}