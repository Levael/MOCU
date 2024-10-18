using NAudio.CoreAudioApi;
using NAudio.Wave;


namespace AudioControl
{
    /// <summary>
    /// Manages the capture of audio data and routes it to a selected AudioOutputDevice's buffer.
    /// This class allows for dynamically switching the target AudioOutputDevice, though only one device can be active at a time.
    /// 
    /// Additionally, the AudioInputDevice is responsible for managing the wave format of the capturing device,
    /// ensuring proper audio processing and compatibility with the selected output.
    /// </summary>
    public class AudioInputDevice : AudioDevice
    {
        private BufferedWaveProvider? buffer { get; set; }
        private WasapiCapture receiver { get; set; }


        public AudioInputDevice(MMDevice mmDevice, WaveFormat unifiedWaveFormat) : base(mmDevice, unifiedWaveFormat)
        {
            buffer = null;

            receiver = new WasapiCapture(device, true, 10); // 10 is same latency as in WasapiOut
            receiver.DataAvailable += OnDataAvailable;
            receiver.WaveFormat = waveFormat;
        }


        public bool StartRecording()
        {
            try
            {
                receiver.StartRecording();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public bool StopRecording()
        {
            try
            {
                receiver.StopRecording();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            buffer.AddSamples(e.Buffer, 0, e.BytesRecorded);

            // tried to add noise cancelling
            /*// Пороговое значение для шумоподавления
            float threshold = 0.5f; // Задайте порог в соответствии с вашими потребностями

            // Проходим по всему буферу и применяем пороговое значение для отсечения шума
            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                // Конвертируем байты в 16-битное значение
                short sample = (short)((e.Buffer[index + 1] << 8) | e.Buffer[index]);

                // Применяем шумоподавление
                float sampleFloat = sample / 32768f; // Конвертируем в значение от -1.0 до 1.0
                if (Math.Abs(sampleFloat) < threshold)
                {
                    sample = 0;
                }

                // Конвертируем обратно в байты
                e.Buffer[index] = (byte)(sample & 0xFF);
                e.Buffer[index + 1] = (byte)(sample >> 8);
            }

            // Добавляем обработанные данные в буфер
            buffer.AddSamples(e.Buffer, 0, e.BytesRecorded);*/
        }

        public void BindToBuffer(BufferedWaveProvider outputBuffer)
        {
            if (outputBuffer == null) throw new ArgumentNullException(nameof(outputBuffer), "Buffer cannot be null");

            buffer = outputBuffer;
        }

        protected override void OnVolumeChanged()
        {
            // Nothing. Currently not implemented for input devices
        }
    }
}
