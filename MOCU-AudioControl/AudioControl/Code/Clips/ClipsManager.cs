using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AudioModule.Daemon
{
    public class ClipsManager
    {
        private Dictionary<AudioClipName, (AudioClipData clipData, float[] preloadedData)> _clips;
        private readonly DevicesManager _devicesManager;

        public event Action ChangesOccurred;

        public ClipsManager(DevicesManager devicesManager)
        {
            _clips = new();
            _devicesManager = devicesManager;
        }

        public IEnumerable<AudioClipData> GetClipsData()
        {
            return _clips.Values.Select(entry => entry.clipData);
        }

        public void PlayAudioClip(PlayAudioClipCommand clipData)
        {
            try
            {
                var device = _devicesManager.GetOutputDevice(clipData.OutputDeviceId);

                if (clipData.InterruptPlayingClips)
                {
                    var interroptedBuffers = device.GetSampleProviders().OfType<ClipSampleProvider>().ToList();

                    foreach (var buffer in interroptedBuffers)
                        device.RemoveSampleProvider(buffer);
                }

                device.AddSampleProvider(new ClipSampleProvider(_clips[clipData.ClipData.name].preloadedData));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred trying 'PlayAudioClip': {ex}");
            }
        }

        public void UpdateClipData(AudioClipData clipData)
        {
            _clips[clipData.name] = (clipData: clipData, preloadedData: LoadAudioFile(path: clipData.fullFilePath, volume: clipData.volume));
            ChangesOccurred?.Invoke();
        }

        private float[] LoadAudioFile(string path, float volume)
        {
            try
            {
                // The maximum Volume increase is 5 times
                var volumeFactor = Math.Clamp(volume / 100, 0f, 5f);

                using var reader = new AudioFileReader(path);
                var resampler = new WdlResamplingSampleProvider(reader, UnifiedAudioFormat.WaveFormat.SampleRate);
                var sampleProvider = resampler.ToStereo();

                var dynamicSamples = new List<float>();
                var readBuffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];
                int samplesRead;

                while ((samplesRead = sampleProvider.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    for (int i = 0; i < samplesRead; i++)
                        dynamicSamples.Add(Math.Clamp(readBuffer[i] * volumeFactor, -1.0f, 1.0f));
                }

                return dynamicSamples.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"This time error is inside 'LoadAudioFile': {ex.StackTrace}, {ex.Message}");
                return new float[0];
            }
        }
    }
}