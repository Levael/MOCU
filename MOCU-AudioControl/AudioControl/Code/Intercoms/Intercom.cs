using NAudio.Wave;


namespace AudioModule.Daemon
{
    public class Intercom
    {
        private readonly IEnumerable<AudioInputDevice> _inputs;
        private readonly IEnumerable<AudioOutputDevice> _outputs;
        private readonly List<BufferedWaveProvider> _buffers;
        private readonly Guid _id;
        private bool _isOn;

        public Intercom(IEnumerable<AudioInputDevice> inputs, IEnumerable<AudioOutputDevice> outputs, Guid id)
        {
            _inputs = inputs;
            _outputs = outputs;
            _buffers = new();
            _id = id;
        }

        // todo: not sure if creating a new object for each request is the best approach.
        // Consider the possibility of creating the object once in the constructor.
        public AudioIntercomData GetIntercomData()
        {
            return new AudioIntercomData
            {
                fromDevices = _inputs.Select(input => input.Id),
                toDevices = _outputs.Select(input => input.Id),
                isOn = _isOn,
                id = _id
            };
        }

        public void Start()
        {
            foreach (var output  in _outputs)
            {
                var buffer = new BufferedWaveProvider(UnifiedAudioFormat.WaveFormat);
                _buffers.Add(buffer);

                output.AddSampleProvider(buffer);

                foreach (var input in _inputs)
                    input.AddBinding(buffer);
            }

            _isOn = true;
        }

        public void Stop()
        {
            foreach (var buffer in _buffers)
            {
                buffer.ReadFully = false;   // outputDevice.mixer will delete them by itself for no data

                foreach (var input in _inputs)
                    input.RemoveBinding(buffer);
            }

            _isOn = false;
        }
    }
}