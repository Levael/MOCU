using DaemonsRelated;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;


namespace AudioModule.Daemon
{
    public class DevicesManager : IMMNotificationClient
    {
        public event Action ChangesOccurred;

        private readonly MMDeviceEnumerator _enumerator;
        
        private readonly Dictionary<Guid, (AudioDeviceData deviceData, AudioInputDevice device)> _inputDevices;
        private readonly Dictionary<Guid, (AudioDeviceData deviceData, AudioOutputDevice device)> _outputDevices;
        private readonly Dictionary<Guid, (IAudioDevice device, float volume)> _originalSettings;

        public DevicesManager()
        {
            _enumerator = new();
            _inputDevices = new();
            _outputDevices = new();
            _originalSettings = new();

            InitDevices();
            SaveOriginalSettings();

            _enumerator.RegisterEndpointNotificationCallback(this);

            //Console.WriteLine(JsonHelper.SerializeJson(new { inputDevices = _inputDevices, outputDevices = _outputDevices }));
        }

        ~DevicesManager()
        {
            _enumerator.UnregisterEndpointNotificationCallback(this);
        }

        // ########################################################################################

        public AudioInputDevice? GetInputDevice(Guid deviceId)
        {
            return _inputDevices.TryGetValue(deviceId, out var deviceTuple) ? deviceTuple.device : null;
        }

        public AudioOutputDevice? GetOutputDevice(Guid deviceId)
        {
            return _outputDevices.TryGetValue(deviceId, out var deviceTuple) ? deviceTuple.device : null;
        }

        public AudioInputDevice? GetDefaultInputDevice()
        {
            var defaultDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);

            if (defaultDevice == null)
                return null;
            else
                return GetInputDevice(Utils.ExtractGuid(defaultDevice.ID));
        }

        public AudioOutputDevice? GetDefaultOutputDevice()
        {
            var defaultDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);

            if (defaultDevice == null)
                return null;
            else
                return GetOutputDevice(Utils.ExtractGuid(defaultDevice.ID));
        }

        public IEnumerable<AudioDeviceData> GetInputDevicesData()
        {
            return _inputDevices.Values.Select(entry => entry.deviceData);
        }

        public IEnumerable<AudioDeviceData> GetOutputDevicesData()
        {
            return _outputDevices.Values.Select(entry => entry.deviceData);
        }

        /// <param name="volume">Can only take values from 0 to 100</param>
        public void ChangeDeviceVolume(Guid deviceId, float volume)
        {
            if (volume < 0 || volume > 100)
                throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be between 0 and 100.");

            var (deviceData, device) = FindDeviceTupleById(deviceId) ?? throw new ArgumentException($"No such device: {deviceId}");

            if (deviceData.ConnectionStatus != AudioDeviceConnectionStatus.Connected)
                throw new Exception("Inactive devices cannot change the volume level");

            if (device.Volume != volume)
            {
                device.Volume = volume;
                deviceData.Volume = volume;
                ChangesOccurred?.Invoke();
            }
        }

        public void RestoreOriginalSettings()
        {
            foreach (var deviceTuple in _originalSettings.Values)
                try { deviceTuple.device.Volume = deviceTuple.volume; } catch { }
        }

        private void SaveOriginalSettings()
        {
            foreach (var deviceKeyValue in _inputDevices)
                _originalSettings[deviceKeyValue.Key] = (device: deviceKeyValue.Value.device, volume: deviceKeyValue.Value.device.Volume);

            foreach (var deviceKeyValue in _outputDevices)
                _originalSettings[deviceKeyValue.Key] = (device: deviceKeyValue.Value.device, volume: deviceKeyValue.Value.device.Volume);
        }

        private (AudioDeviceData deviceData, IAudioDevice device)? FindDeviceTupleById(Guid deviceId)
        {
            return _inputDevices.TryGetValue(deviceId, out var inputDevice) ? inputDevice :
                   _outputDevices.TryGetValue(deviceId, out var outputDevice) ? outputDevice :
                   null;
        }

        private void InitDevices()
        {
            foreach (var device in _enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active))
                AddNewDevice(device);
        }

        private void AddNewDevice(MMDevice device)
        {
            try
            {
                var deviceData = new AudioDeviceData
                {
                    Id = Utils.ExtractGuid(device.ID),
                    Name = device.FriendlyName,
                    Volume = device.AudioEndpointVolume.MasterVolumeLevelScalar * 100,
                    Type = device.DataFlow == DataFlow.Capture ? AudioDeviceType.Input : AudioDeviceType.Output,
                    ConnectionStatus = device.State == DeviceState.Active ? AudioDeviceConnectionStatus.Connected : AudioDeviceConnectionStatus.Disconnected
                };

                switch (deviceData.Type)
                {
                    case AudioDeviceType.Input:
                        var audioInputDevice = new AudioInputDevice(device);
                        _inputDevices[deviceData.Id] = (deviceData: deviceData, device: audioInputDevice);
                        break;

                    case AudioDeviceType.Output:
                        var audioOutputDevice = new AudioOutputDevice(device);
                        _outputDevices[deviceData.Id] = (deviceData: deviceData, device: audioOutputDevice);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error occurred while trying to add new device to dict." +
                    $"Device caused an exception: {device.FriendlyName}. Error: {ex}");
            }
        }

        // ########################################################################################

        public void OnDeviceStateChanged(string deviceGuidStr, DeviceState newState)
        {
            Console.WriteLine($"Device {deviceGuidStr} state changed to {newState}");

            var deviceGuid = Utils.ExtractGuid(deviceGuidStr);
            var result = FindDeviceTupleById(deviceGuid);

            if (result == null)
            {
                if (newState == DeviceState.Active)
                    AddNewDevice(_enumerator.GetDevice(deviceGuidStr));
            }
            else
            {
                var (deviceData, device) = result.Value;
                deviceData.ConnectionStatus = (newState == DeviceState.Active) ? AudioDeviceConnectionStatus.Connected : AudioDeviceConnectionStatus.Disconnected;

                // reconnected
                if (deviceData.ConnectionStatus == AudioDeviceConnectionStatus.Connected)
                    device.Reinitialize();
            }
            
            ChangesOccurred?.Invoke();
        }

        #region Unused but required for interface implementation
        public void OnDeviceAdded(string pwstrDeviceId) { }
        public void OnDeviceRemoved(string deviceId) { }
        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }
        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId) { }
        #endregion Unused but required for interface implementation
    }
}