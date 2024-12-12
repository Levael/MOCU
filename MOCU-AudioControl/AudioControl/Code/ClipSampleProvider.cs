using System;
using System.Numerics;
using NAudio.Wave;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace AudioModule.Daemon
{

    public class ClipSampleProvider : ISampleProvider
    {
        private readonly float[] audioData; // link for preloaded audio data
        private int readPosition;
        private bool isCompleted;

        public WaveFormat WaveFormat { get; }

        public event EventHandler PlaybackCompleted;

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
                PlaybackCompleted?.Invoke(this, EventArgs.Empty);
            }

            return samplesToCopy;
        }
    }

}