using NAudio.Wave;


namespace AudioModule.Daemon
{
    public static class UnifiedAudioFormat
    {
        // 32 bit IEEFloat: 44100Hz 2 channels
        public static WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

        // milliseconds
        public static int BufferSize { get; } = 10;    // optimum is 10 (todo: test it, not sure)
    }
}