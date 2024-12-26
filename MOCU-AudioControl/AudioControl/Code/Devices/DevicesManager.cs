using NAudio.CoreAudioApi;
using System.Text.RegularExpressions;
using NAudio.CoreAudioApi.Interfaces;


namespace AudioModule.Daemon
{
    public class DevicesManager : IMMNotificationClient
    {
        public event Action ChangesOccurred;

        private readonly MMDeviceEnumerator _enumerator;
        
        private readonly Dictionary<Guid, (AudioDeviceData deviceData, AudioInputDevice device)> _inputDevices;
        private readonly Dictionary<Guid, (AudioDeviceData deviceData, AudioOutputDevice device)> _outputDevices;

        // todo: think about them later
        /*private AudioInputDevice _defaultInputDevice;
        private AudioOutputDevice _defaultOutputDevice;*/

        public DevicesManager()
        {
            _enumerator = new();
            _inputDevices = new();
            _outputDevices = new();

            InitDevices();

            _enumerator.RegisterEndpointNotificationCallback(this);
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
                ChangesOccurred?.Invoke();
            }
        }

        private Guid ExtractGuid(string deviceId)
        {
            var match = Regex.Match(deviceId, @"\{[0-9a-fA-F\-]{36}\}");

            if (match.Success)
            {
                string guidString = match.Value.Trim('{', '}');

                if (Guid.TryParse(guidString, out var deviceGuid))
                    return deviceGuid;
            }

            throw new Exception($"Coudn't exctract Guid from deviceId: {deviceId}");
        }

        private (AudioDeviceData deviceData, IAudioDevice device)? FindDeviceTupleById(Guid deviceId)
        {
            return _inputDevices.TryGetValue(deviceId, out var inputDevice) ? inputDevice :
                   _outputDevices.TryGetValue(deviceId, out var outputDevice) ? outputDevice :
                   null;
        }

        private void InitDevices()
        {
            /*var defaultCaptureDeviceId = ExtractGuid(_enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console)?.ID);
            var defaultRenderDeviceId = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console)?.ID;*/

            foreach (var device in _enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active))
                AddNewDevice(device);
        }

        private void AddNewDevice(MMDevice device)
        {
            try
            {
                var deviceData = new AudioDeviceData
                {
                    Id = ExtractGuid(device.ID),
                    Name = device.FriendlyName,
                    Volume = device.AudioEndpointVolume.MasterVolumeLevelScalar * 100,
                    Type = device.DataFlow == DataFlow.Capture ? AudioDeviceType.Input : AudioDeviceType.Output,
                    ConnectionStatus = device.State == DeviceState.Active ? AudioDeviceConnectionStatus.Connected : AudioDeviceConnectionStatus.Disconnected
                };

                switch (deviceData.Type)
                {
                    case AudioDeviceType.Input:
                        var audioInputDevice = new AudioInputDevice(device);
                        /*if (device.ID == defaultCaptureDeviceId)
                            _defaultInputDevice = audioInputDevice;*/
                        _inputDevices[deviceData.Id] = (deviceData: deviceData, device: audioInputDevice);
                        break;

                    case AudioDeviceType.Output:
                        var audioOutputDevice = new AudioOutputDevice(device);
                        /*if (device.ID == defaultRenderDeviceId)
                            _defaultOutputDevice = audioOutputDevice;*/
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

            var deviceGuid = ExtractGuid(deviceGuidStr);
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
            }
            
            ChangesOccurred?.Invoke();
        }

        // No actual need, but must be implemented
        public void OnDeviceAdded(string pwstrDeviceId) { }
        public void OnDeviceRemoved(string deviceId) { }
        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId) { }
        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }
    }
}