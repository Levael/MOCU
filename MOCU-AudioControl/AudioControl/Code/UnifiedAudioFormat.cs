using NAudio.Wave;


namespace AudioModule.Daemon
{
    public static class UnifiedAudioFormat
    {
        /// <summary>
        /// The default audio format: 32-bit IEEE Float, 44100 Hz, 2 channels.
        /// This format is widely supported and offers high audio quality.
        /// </summary>
        public static WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

        /// <summary>
        /// Buffer size in milliseconds. Optimal value is 10 ms for low latency.
        /// Increase this value if you experience audio artifacts (at the cost of greater delay).
        /// </summary>
        public static int BufferSize { get; } = 10;

        /// <summary>
        /// Determines whether to use event-driven synchronization with WASAPI.
        /// Set to 'true' for lower latency. Ensure the audio device and drivers support this feature 
        /// to avoid potential compatibility issues.
        /// </summary>
        public static bool UseEventSync { get; } = true;


        // Alternative format for better performance (mono, 22050 Hz).
        // Replace if mono and lower quality are acceptable for your application.
        // public static WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(22050, 1);
    }
}