using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace AudioControl
{
    public class NotificatorForAudioDevicesParameters : IMMNotificationClient
    {
        private AudioDevicesParameters _caller;

        public NotificatorForAudioDevicesParameters(AudioDevicesParameters caller)
        {
            _caller = caller;
        }



        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            _caller.OnDeviceStateChanged(deviceId, newState);
        }



        #region UNUSED METHODS (still have to be because of the interface)
        public void OnDeviceAdded(string deviceId) { }

        public void OnDeviceRemoved(string deviceId) { }

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId) { }

        public void OnPropertyValueChanged(string deviceId, PropertyKey key) { }
        #endregion
    }

}
