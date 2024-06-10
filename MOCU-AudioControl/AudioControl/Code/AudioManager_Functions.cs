using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace AudioControl
{
    // Functions part

    public partial class AudioManager
    {
        private Dictionary<string, AudioOutputDevice> InitOutputDevicesDictionary(MMDeviceCollection outputDevices)
        {
            Dictionary<string, AudioOutputDevice>? audioOutputsDictionary = new();

            foreach (var device in outputDevices)
            {
                audioOutputsDictionary.Add(device.FriendlyName, new AudioOutputDevice(device, unifiedWaveFormat));
            }

            return audioOutputsDictionary;
        }

        private Dictionary<string, AudioInputDevice> InitInputDevicesDictionary(MMDeviceCollection inputDevices)
        {
            Dictionary<string, AudioInputDevice>? audioInputsDictionary = new();

            foreach (var device in inputDevices)
            {
                audioInputsDictionary.Add(device.FriendlyName, new AudioInputDevice(device, unifiedWaveFormat));
            }

            return audioInputsDictionary;
        }

        private void LoadAudioFiles()
        {
            preLoadedAudioFiles = new();

            foreach (var audioFileName in audioFileNames)
            {
                using var reader = new AudioFileReader(Path.Combine(pathToAudioFiles, audioFileName));
                var resampler = new WdlResamplingSampleProvider(reader, unifiedWaveFormat.SampleRate);
                var sampleProvider = resampler.ToStereo();

                var wholeFile = new List<byte>();

                float[] readBuffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];
                byte[] byteBuffer = new byte[readBuffer.Length * 4];
                int samplesRead;

                while ((samplesRead = sampleProvider.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    Buffer.BlockCopy(readBuffer, 0, byteBuffer, 0, samplesRead * 4);
                    wholeFile.AddRange(byteBuffer.Take(samplesRead * 4));
                }

                preLoadedAudioFiles.Add(audioFileName, wholeFile.ToArray());
            }
        }

        private void UpdateIntercom()
        {
            incomingStream.UpdateDevices(
                audioInputDevice: audioDevicesParameters.audioInputDevice_Participant,
                audioOutputDevice: audioDevicesParameters.audioOutputDevice_Researcher
            );

            outgoingStream.UpdateDevices(
                audioInputDevice: audioDevicesParameters.audioInputDevice_Researcher,
                audioOutputDevice: audioDevicesParameters.audioOutputDevice_Participant
            );
        }

        private (bool errorOccured, object? extraData) UpdateAllDevicesCollections()
        {
            var enumerator = new MMDeviceEnumerator();

            var inputDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            var outputDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            var audioInputsDictionary = InitInputDevicesDictionary(inputDevices);
            var audioOutputsDictionary = InitOutputDevicesDictionary(outputDevices);

            var result = audioDevicesParameters.UpdateDevicesDictionaries(inputsDict: audioInputsDictionary, outputsDict: audioOutputsDictionary);
            return result;
        }

        private MMDevice GetDeviceByItsName(string deviceName, MMDeviceCollection devices)
        {
            int index = -1;

            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i].FriendlyName == deviceName)
                {
                    index = i;
                    break;
                }
            }

            return devices[index];
        }

        /// <summary>
        /// Copies pre-prepared and unified audio to a buffer subscribed to the mixer.
        /// If something is being played at the moment of calling the function, it purposely cuts it off and puts a newer one
        /// </summary>
        private void PlayAudioFile(PlayAudioFile_Command commandData)
        {
            var audioData = preLoadedAudioFiles[commandData.AudioFileName];
            var buffer = audioDevicesParameters.audioOutputsDictionary[commandData.AudioOutputDeviceName].bufferForSingleAudioPlay;

            buffer.ClearBuffer();
            buffer.AddSamples(audioData, 0, audioData.Length);
        }

    }
}
