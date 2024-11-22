using NAudio.Wave;
using System.Collections.Generic;
using System;


namespace AudioModule.Daemon
{
    public class InputStreamSampleProvider : ISampleProvider
    {
        private readonly WaveFormat waveFormat;
        private readonly Queue<float> sampleQueue;
        private readonly float[] sharedBuffer;

        public WaveFormat WaveFormat => waveFormat;

        /*public event EventHandler KillMe;*/

        public InputStreamSampleProvider()
        {
            this.waveFormat = UnifiedAudioFormat.WaveFormat;
            this.sampleQueue = new Queue<float>();
        }

        // the method should be as fast as possible, as it is executed in the capture thread
        public void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            for (int i = 0; i < e.BytesRecorded; i += 4)
            {
                float sample = BitConverter.ToSingle(e.Buffer, i);
                sampleQueue.Enqueue(sample);
            }
        }

        /*public void Remove()
        {
            KillMe?.Invoke(this, EventArgs.Empty);
        }*/

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = 0;

            while (samplesRead < count && sampleQueue.Count > 0)
            {
                buffer[offset + samplesRead] = sampleQueue.Dequeue();
                samplesRead++;
            }

            return samplesRead;
        }
    }

}