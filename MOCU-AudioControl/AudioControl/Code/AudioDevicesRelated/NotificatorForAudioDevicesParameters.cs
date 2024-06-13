using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace AudioControl
{
    public class NotificatorForAudioDevicesParameters : IMMNotificationClient
    {
        private AudioDevicesParameters _notificationTarget;

        public NotificatorForAudioDevicesParameters(AudioDevicesParameters notificationTarget)
        {
            _notificationTarget = notificationTarget;
        }



        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            _notificationTarget.OnDeviceStateChanged(deviceId, newState);
        }



        #region UNUSED METHODS (still have to be because of the interface)
        public void OnDeviceAdded(string deviceId) { }

        public void OnDeviceRemoved(string deviceId) { }

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId) { }

        public void OnPropertyValueChanged(string deviceId, PropertyKey key) { }
        #endregion
    }

}
