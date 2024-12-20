using NAudio.Wave;


namespace AudioModule.Daemon
{
    public class ClipSampleProvider : ISampleProvider
    {
        private readonly float[] audioData; // link for preloaded audio data
        private int readPosition;
        private bool isCompleted;   // the whole class behaves kinda as 'ReadFully = false'

        public WaveFormat WaveFormat { get; }
        public event Action PlaybackCompleted;

        public ClipSampleProvider(float[] clipData)
        {
            this.audioData = clipData ?? throw new ArgumentNullException(nameof(clipData));
            this.WaveFormat = UnifiedAudioFormat.WaveFormat;
            this.readPosition = 0;
            this.isCompleted = false;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (isCompleted)
                return 0;

            int samplesAvailable = audioData.Length - readPosition;
            int samplesToCopy = Math.Min(samplesAvailable, count);

            Array.Copy(audioData, readPosition, buffer, offset, samplesToCopy);
            readPosition += samplesToCopy;

            if (samplesToCopy < count)
            {
                isCompleted = true;
                PlaybackCompleted?.Invoke();
            }

            return samplesToCopy;
        }
    }
}