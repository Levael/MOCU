using NAudio.Wave.SampleProviders;
using NAudio.Wave;

namespace AudioControl
{
    // Functions part

    public partial class AudioManager
    {
        private void LoadAudioFiles()
        {
            preLoadedAudioFiles = new();

            foreach (var audioFileName in audioFileNames)
                preLoadedAudioFiles.Add(audioFileName, LoadAudioFile(audioFileName));
        }

        private float[] LoadAudioFile(string audioFileName)
        {
            using var reader = new AudioFileReader(Path.Combine(pathToAudioFiles, audioFileName));
            var resampler = new WdlResamplingSampleProvider(reader, audioDevicesParameters.unifiedWaveFormat.SampleRate);
            var sampleProvider = resampler.ToStereo();

            int totalSamples = (int)(reader.Length / sizeof(float));
            var wholeFile = new float[totalSamples];
            int totalSamplesRead = 0;

            float[] readBuffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];
            int samplesRead;

            while ((samplesRead = sampleProvider.Read(readBuffer, 0, readBuffer.Length)) > 0)
            {
                Array.Copy(readBuffer, 0, wholeFile, totalSamplesRead, samplesRead);
                totalSamplesRead += samplesRead;
            }

            if (totalSamplesRead < totalSamples)
                Array.Resize(ref wholeFile, totalSamplesRead);

            return wholeFile;
        }

        private void UpdateIntercom()
        {
            /*incomingStream.UpdateDevices(
                audioInputDevice: audioDevicesParameters.audioInputDevice_Participant,
                audioOutputDevice: audioDevicesParameters.audioOutputDevice_Researcher
            );

            outgoingStream.UpdateDevices(
                audioInputDevice: audioDevicesParameters.audioInputDevice_Researcher,
                audioOutputDevice: audioDevicesParameters.audioOutputDevice_Participant
            );*/
        }

        /// <summary>
        /// Copies pre-prepared and unified audio to a buffer subscribed to the mixer.
        /// If something is being played at the moment of calling the function, it purposely cuts it off and puts a newer one
        /// </summary>
        private void PlayAudioFile(PlayAudioFile_CommandDetails commandData)
        {
            /*var audioData = preLoadedAudioFiles[commandData.audioFileName];
            var buffer = audioDevicesParameters.audioOutputsDictionary[commandData.audioOutputDeviceName].bufferForSingleAudioPlay;

            buffer.ClearBuffer();
            buffer.AddSamples(audioData, 0, audioData.Length);*/
        }

    }
}


/*
 Да, конечно! Можно сделать так, чтобы сам ClipSampleProvider отслеживал окончание воспроизведения и генерировал событие. В таком случае вам не нужно создавать отдельный класс для микширования — вся логика удаления будет происходить в одном месте, при добавлении провайдера в MixingSampleProvider. Это позволит вам сохранять основную структуру кода и просто добавлять обработчик событий в провайдеры при их создании
 */