using NAudio.CoreAudioApi;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;

using DaemonsRelated.DaemonPart;


/*
 * Currently 'PlayClips' has no feedback. Maybe later add event 'ClipPlayed' or smth like that.
 * 'HandleIntercoms', 'UpdateDevicesData' and 'UpdateClipsData' always return all currently running intercoms so host side is always up to date.
 */


namespace AudioModule.Daemon
{
    public class AudioDaemon : IDaemonLogic
    {
        public event Action<string> TerminateDaemon;

        private DevicesManager _devicesManager;
        private IntercomsManager _intercomsManager;

        private AudioDaemonSideBridge _hostAPI;
        private Dictionary<AudioClipName, (AudioClipData clipData, float[] preloadedData)> _clips;

        // test
        //private AudioOutputDevice _defaultOutputDevice;

        public AudioDaemon(AudioDaemonSideBridge hostAPI)
        {
            _devicesManager = new();
            _devicesManager.ChangesOccurred += OnDevicesChanged;

            _intercomsManager = new(_devicesManager);
            _intercomsManager.ChangesOccurred += OnIntercomsChanged;

            _clips = new();

            _hostAPI = hostAPI;

            _hostAPI.PlayClips              += PlayAudioClips;
            _hostAPI.UpdateIntercomStates   += HandleIntercoms;
            _hostAPI.UpdateDevicesData      += UpdateDevicesData;
            _hostAPI.UpdateClipsData        += UpdateClipsData;

            //_defaultOutputDevice = new AudioOutputDevice(new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia));
        }

        public void Run()
        {
            _hostAPI.StartCommunication();
        }

        public void DoBeforeExit()
        {
            // todo: undo every changes made to OS (audio devices Volume etc)
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
                _intercomsManager.HandleIntercomCommand(intercom);
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
            try
            {
                // todo: add null check
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

        // TESTS ##################################################################################


        private void TestMethod()
        {
            /*try
            {
                // Play 'PingDevice' clip using default output
                _defaultOutputDevice.AddSampleProvider(new ClipSampleProvider(_clips[AudioClipName.PingDevice].preloadedData));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred trying 'TestMethod': {ex}");
            }*/
        }

        private float[] LoadAudioFile(string path, float volume)
        {
            try
            {
                // The maximum Volume increase is 5 times
                var volumeFactor = Math.Clamp(volume / 100, 0f, 5f);

                using var reader = new AudioFileReader(Path.Combine(path));
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

        private void OnDevicesChanged()
        {
            // todo: inform the server about the changes
            Console.WriteLine("Some device(s) changed");
        }

        private void OnIntercomsChanged()
        {
            // todo: inform the server about the changes
            Console.WriteLine("Some intercom(s) changed");
        }
    }
}