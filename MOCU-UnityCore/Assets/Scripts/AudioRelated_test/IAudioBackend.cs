using System;


namespace AudioModule
{
    public interface IAudioBackend
    {
        void PlayAudioClip(IAudioOutputDevice device, AudioClip clipName, bool doCropPrevClip);

        void StartIntercomStream(IAudioInputDevice input, IAudioOutputDevice output);

        void StopIntercomStream(IAudioInputDevice input, IAudioOutputDevice output);
    }
}
