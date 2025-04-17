using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AudioModule.Daemon
{
    public class IntercomsManager
    {
        private Dictionary<Guid, Intercom> _intercoms;
        private readonly DevicesManager _devicesManager;

        public event Action ChangesOccurred;

        public IntercomsManager(DevicesManager devicesManager)
        {
            _intercoms = new();
            _devicesManager = devicesManager;
        }

        public IEnumerable<AudioIntercomData> GetIntercomsData()
        {
            return _intercoms.Values.Select(entry => entry.GetIntercomData());
        }

        public void HandleIntercomCommand(AudioIntercomData data)
        {
            if (_intercoms.ContainsKey(data.id) && data.isOn == false)
                DestroyIntercomStream(data);

            if (!_intercoms.ContainsKey(data.id) && data.isOn == true)
                CreateIntercomStream(data);

            // todo: maybe send an error response to server if request wasn't proper
        }

        private void CreateIntercomStream(AudioIntercomData data)
        {
            try
            {
                var inputs = data.fromDevices
                    .Select(id => _devicesManager.GetInputDevice(id))
                    .OfType<AudioInputDevice>();

                var outputs = data.toDevices
                    .Select(id => _devicesManager.GetOutputDevice(id))
                    .OfType<AudioOutputDevice>();

                if (inputs is null || !inputs.Any())
                    throw new Exception("No available inputs");

                if (outputs is null || !outputs.Any())
                    throw new Exception("No available outputs");

                var intercom = new Intercom(inputs: inputs, outputs: outputs, id: data.id);
                intercom.Start();

                _intercoms.Add(data.id, intercom);
                ChangesOccurred?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred in 'CreateIntercomStream': {ex}");
                DestroyIntercomStream(data);
            }
        }

        private void DestroyIntercomStream(AudioIntercomData data)
        {
            try
            {
                _intercoms[data.id].Stop();
                _intercoms.Remove(data.id);
                ChangesOccurred?.Invoke();
            }
            catch
            {
                Console.WriteLine("Error occurred in 'DestroyIntercomStream'.");
            }
        }
    }
}