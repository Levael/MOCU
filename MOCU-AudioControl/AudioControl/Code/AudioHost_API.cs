using DaemonsRelated;
using System;
using System.Collections.Generic;


namespace AudioModule
{
    public interface AudioHost_API  // Is a mirror of 'AudioDaemon_API'
    {
        // commands from Host
        event Action<IEnumerable<PlayAudioClipCommand>> PlayClips;
        event Action<IEnumerable<AudioDeviceData>> UpdateDevicesData;
        event Action<IEnumerable<AudioClipData>> UpdateClipsData;
        event Action<IEnumerable<AudioIntercomData>> UpdateIntercomStates;

        // responses from Daemon
        void DevicesDataChanged(IEnumerable<AudioDeviceData> devicesData);
        void ClipsDataChanged(IEnumerable<AudioClipData> clipsData);
        void IntercomStatesChanged(IEnumerable<AudioIntercomData> intercomsData);
        void ErrorsOccurred(IEnumerable<DaemonErrorReport> errors);
    }
}