using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DaemonsRelated;
using DaemonsRelated.DaemonPart;
using NAudio.CoreAudioApi;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using Microsoft.VisualBasic;
using System.Reflection.PortableExecutable;


/*
 * Currently 'PlayClips' has no feedback. Maybe later add event 'ClipPlayed' or smth like that.
 * 'HandleIntercoms', 'UpdateDevicesData' and 'UpdateClipsData' always return all currently running intercoms so host side is always up to date.
 */


namespace AudioModule.Daemon
{
    public class AudioDaemon : IDaemonLogic
    {
        public event Action<string> TerminateDaemon;

        private AudioDaemonSideBridge _hostAPI;
        private Dictionary<string, (AudioDeviceData deviceData, AudioInputDevice device)> _inputDevices;
        private Dictionary<string, (AudioDeviceData deviceData, AudioOutputDevice device)> _outputDevices;
        private Dictionary<AudioClipName, (AudioClipData clipData, float[] preloadedData)> _clips;
        private Dictionary<(string senderDeviceId, string recieverDeviceId), AudioIntercomData> _intercoms;

        // test
        private AudioOutputDevice _defaultOutputDevice;

        public AudioDaemon(AudioDaemonSideBridge hostAPI)
        {
            _clips = new();
            _intercoms = new();
            _inputDevices = new();
            _outputDevices = new();

            _hostAPI = hostAPI;

            _hostAPI.PlayClips              += PlayAudioClips;
            _hostAPI.UpdateIntercomStates   += HandleIntercoms;
            _hostAPI.UpdateDevicesData      += UpdateDevicesData;
            _hostAPI.UpdateClipsData        += UpdateClipsData;

            _defaultOutputDevice = new AudioOutputDevice(new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia));
        }

        public void Run()
        {
            _hostAPI.StartCommunication();
        }

        public void DoBeforeExit()
        {
            // todo: undo every changes made to OS (audio devices volume etc)
        }

        // ########################################################################################

        private void PlayAudioClips(IEnumerable<PlayAudioClipCommand> clips)
        {
            foreach (var clip in clips)
                PlayAudioClip(clip);
        }

        private void HandleIntercoms(IEnumerable<AudioIntercomData> intercoms)
        {
            foreach (var intercom in intercoms)
                HandleIntercom(intercom);

            _hostAPI.IntercomStatesChanged(_intercoms.Values);
        }

        private void UpdateDevicesData(IEnumerable<AudioDeviceData> devices)
        {
            // temp
            Console.WriteLine("UpdateDevicesData logic");
        }

        private void UpdateClipsData(IEnumerable<AudioClipData> clips)
        {
            foreach(var clip in clips)
                _clips[clip.name] = (clipData: clip, preloadedData: LoadAudioFile(path: clip.fullFilePath, volume: clip.volume));

            _hostAPI.ClipsDataChanged(_clips.Values.Select(entry => entry.clipData));
        }

        // ########################################################################################

        private void PlayAudioClip(PlayAudioClipCommand clipData)
        {
            // temp
            //Console.WriteLine($"Played clip '{clipData.ClipData.name}'");
            TestMethod();
            Console.WriteLine($"Played test clip");
        }

        private void HandleIntercom(AudioIntercomData intercom)
        {
            // temp
            Console.WriteLine($"Handled intercom. It's now is ON: {intercom.isOn}");
        }

        // TESTS ##################################################################################

        private void AddOutputDevice(MMDevice device)
        {
            if (_outputDevices.ContainsKey(device.ID))
                return;

            var deviceData = new AudioDeviceData
            {
                id = device.ID,
                volume = device.AudioEndpointVolume.MasterVolumeLevelScalar * 100,
                type = AudioDeviceType.Output,
                connectionStatus = device.State == DeviceState.Active
                ? AudioDeviceConnectionStatus.Connected
                : AudioDeviceConnectionStatus.Disconnected
            };

            var audioOutputDevice = new AudioOutputDevice(device);
            _outputDevices.Add(deviceData.id, (deviceData: deviceData, device: audioOutputDevice));
        }

        private void TestMethod()
        {
            try
            {
                // Play 'PingDevice' clip using default output
                _defaultOutputDevice.AddSampleProvider(new ClipSampleProvider(_clips[AudioClipName.CorrectAnswer].preloadedData));

                /*var bufferedWaveProvider = new BufferedWaveProvider(UnifiedAudioFormat.WaveFormat);
                byte[] byteData = new byte[_clips[AudioClipName.PingDevice].preloadedData.Length * 4];
                Buffer.BlockCopy(_clips[AudioClipName.PingDevice].preloadedData, 0, byteData, 0, byteData.Length);
                bufferedWaveProvider.AddSamples(byteData, 0, byteData.Length);*/

                /*using var audioFileReader = new AudioFileReader(_clips[AudioClipName.PingDevice].clipData.fullFilePath);
                var sampleProvider = audioFileReader.ToSampleProvider();
                var resampler = new WdlResamplingSampleProvider(sampleProvider, UnifiedAudioFormat.WaveFormat.SampleRate);
                var sampleProvider2 = resampler.ToStereo();

                _defaultOutputDevice.AddSampleProvider(sampleProvider2);*/

                /*var signalGenerator = new SignalGenerator(44100, 2) // 44100 Гц, 2 канала
                {
                    Gain = 0.6,                  // Громкость (0.0 - 1.0)
                    Frequency = 440.0,           // Частота сигнала (Гц)
                    Type = SignalGeneratorType.Sin // Тип волны (синусоида)
                };

                _defaultOutputDevice.AddSampleProvider(signalGenerator);*/

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred trying 'TestMethod': {ex}");
            }
        }

        private float[] LoadAudioFile(string path, float volume)
        {
            try
            {
                // The maximum volume increase is 5 times
                var volumeFactor = Math.Clamp(volume / 100, 0f, 5f);

                using var reader = new AudioFileReader(Path.Combine(path));
                var resampler = new WdlResamplingSampleProvider(reader, UnifiedAudioFormat.WaveFormat.SampleRate);
                var sampleProvider = resampler.ToStereo();

                // Estimated array size to minimize allocations
                /*int estimatedSize = (int)(reader.Length / reader.WaveFormat.BlockAlign);
                var wholeFile = new float[estimatedSize];*/

                int totalSamples = 0;
                List<float> dynamicSamples = new();

                float[] readBuffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];
                int samplesRead;

                while ((samplesRead = sampleProvider.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    // Ensure there's enough space in the array
                    /*if (totalSamples + samplesRead > wholeFile.Length)
                        Array.Resize(ref wholeFile, wholeFile.Length * 2);*/

                    for (int i = 0; i < samplesRead; i++)
                        dynamicSamples.Add(Math.Clamp(readBuffer[i] * volumeFactor, -1.0f, 1.0f));
                        //wholeFile[totalSamples++] = Math.Clamp(readBuffer[i] * volumeFactor, -1.0f, 1.0f);
                }

                // Return a trimmed array with the exact number of elements
                /*Array.Resize(ref wholeFile, totalSamples);
                return wholeFile;*/
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