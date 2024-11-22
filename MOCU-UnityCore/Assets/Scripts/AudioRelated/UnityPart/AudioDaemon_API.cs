/*
    Is a mirror of 'AudioHost_API'
 */


using System;
using System.Collections;
using System.Collections.Generic;
using DaemonsRelated;


namespace AudioModule
{
    public interface AudioDaemon_API
    {
        void PlayAudioClips(IEnumerable<PlayAudioClipCommand> playCommandsData);
        void UpdateAudioDevices(IEnumerable<AudioDeviceData> devicesData);
        void UpdateAudioClips(IEnumerable<AudioClipData> clipsData);
        void StartIntercoms(IEnumerable<AudioIntercomData> intercomsData);
        void StopIntercoms(IEnumerable<AudioIntercomData> intercomsData);

        event Action<IEnumerable<AudioDeviceData>> AudioDevicesHaveChanged;
        event Action<IEnumerable<AudioClipData>> AudioClipsHaveChanged;
        event Action<IEnumerable<AudioIntercomData>> IntercomsHaveChanged;
        event Action<IEnumerable<DaemonErrorReport>> ErrorsOccurred;
    }
}