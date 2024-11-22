/*
    Is a mirror of 'AudioDaemon_API'
*/


using DaemonsRelated;
using System;
using System.Collections.Generic;


namespace AudioModule
{
    public interface AudioHost_API
    {
        event Action<IEnumerable<PlayAudioClipCommand>> PlayAudioClips;
        event Action<IEnumerable<AudioDeviceData>> UpdateAudioDevices;
        event Action<IEnumerable<AudioClipData>> UpdateAudioClips;
        event Action<IEnumerable<AudioIntercomData>> StartIntercoms;
        event Action<IEnumerable<AudioIntercomData>> StopIntercoms;

        void AudioDevicesHaveChanged(IEnumerable<AudioDeviceData> devicesData);
        void AudioClipsHaveChanged(IEnumerable<AudioClipData> clipsData);
        void IntercomsHaveChanged(IEnumerable<AudioIntercomData> intercomsData);
        void ErrorsOccurred(IEnumerable<DaemonErrorReport> errors);
    }
}