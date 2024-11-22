using NAudio.Wave;


namespace AudioModule.Daemon
{
    public static class UnifiedAudioFormat
    {
        // 32 bit IEEFloat: 44100Hz 2 channels
        public static WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);

        // milliseconds
        public static int BufferSize { get; } = 100;
    }
}