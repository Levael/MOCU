using System;


namespace AudioModule
{
    public interface AudioDaemon_API
    {
        void PlayAudioClip(PlayAudioClip_MessageDetails data);
        void UpdateParameters(UpdateParameters_MessageDetails data);
        void StartIntercom(Guid fromDevice, Guid toDevice);
        void StopIntercom(Guid fromDevice, Guid toDevice);

        event Action<UpdateParameters_MessageDetails> ParametersHaveChanged;
        event Action<DaemonErrorReport> ErrorOccurred;
    }

    public interface AudioHost_API
    {
        event Action<PlayAudioClip_MessageDetails> PlayAudioClip;
        event Action<UpdateParameters_MessageDetails> UpdateParameters;
        event Action<Guid, Guid> StartIntercom;
        event Action<Guid, Guid> StopIntercom;

        void ParametersHaveChanged(UpdateParameters_MessageDetails parameters);
        void ErrorOccurred(DaemonErrorReport error);
    }
}