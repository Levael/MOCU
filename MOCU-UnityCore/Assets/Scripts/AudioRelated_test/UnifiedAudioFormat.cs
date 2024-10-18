using NAudio.Wave;


namespace AudioModule_NAudio
{
    public static class UnifiedAudioFormat
    {
        // 32 bit IEEFloat: 44100Hz 2 channels
        public static WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);

        // ms
        public static int BufferSize { get; } = 100;
    }
}
