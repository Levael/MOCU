using NAudio.Wave;


namespace AudioModule.Daemon
{
    public class Intercom
    {
        private IEnumerable<AudioInputDevice> _inputs;
        private IEnumerable<AudioOutputDevice> _outputs;
        private List<BufferedWaveProvider> _buffers;
        private Guid _id;

        public Intercom(IEnumerable<AudioInputDevice> inputs, IEnumerable<AudioOutputDevice> outputs, Guid id)
        {
            _inputs = inputs;
            _outputs = outputs;
            _buffers = new();
            _id = id;
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
        }

        public void Stop()
        {
            foreach (var buffer in _buffers)
            {
                buffer.ReadFully = false;   // outputDevice.mixer will delete them by itself for no data

                foreach (var input in _inputs)
                    input.RemoveBinding(buffer);
            }
        }
    }
}