using System;
using System.Collections.Generic;

using InputModule;


namespace AudioModule
{
    public interface AudioHandler_API
    {
        IEnumerable<AudioDeviceData> GetInputs();
        IEnumerable<AudioDeviceData> GetOutputs();
        void PlayAudioClip(DeviceUser user, AudioClipName clip);
        void PingDevice(Guid deviceId);
        void UpdateAudioDevice(AudioDeviceData deviceData);
        void UpdateAudioClip(AudioClipData clipData);
        void StartIntercom(DeviceUser fromUser, DeviceUser toUser); // todo: thing about identification of different streams
        void StopIntercom(DeviceUser fromUser, DeviceUser toUser);

        event Action<AudioDeviceData> AudioDeviceHasChanged;
        event Action<AudioClipData> AudioClipHasChanged;
    }
}