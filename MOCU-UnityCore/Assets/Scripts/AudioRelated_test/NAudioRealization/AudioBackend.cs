using AudioModule;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;


namespace AudioModule_NAudio
{
    public class AudioBackend
    {
        private Dictionary<(AudioInputDevice, AudioOutputDevice), InputStreamSampleProvider> _intercomStreams;


        public AudioBackend()
        {
            _intercomStreams = new();
        }

        public void Test()
        {
            var enumerator = new MMDeviceEnumerator();
            //var audioOutputsDictionary = new Dictionary<string, AudioOutputDevice>();
            var outputDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            var _signalGenerator = new SignalGenerator()
            {
                Gain = 0.2, // Уровень громкости
                Frequency = 440, // Частота синусоиды в Гц (440 Гц — нота Ля)
                Type = SignalGeneratorType.Sin // Тип сигнала — синусоида
            };

            // Инициализируем WasapiOut без указания параметров (дефолтное устройство вывода)
            var _player = new WasapiOut();

            // Передаем генератор синусоиды в WasapiOut
            _player.Init(_signalGenerator.ToWaveProvider());

            // Начинаем воспроизведение
            _player.Play();

            /*foreach (var device in outputDevices)
            {

                *//*ComHelper.RunInSTAThread(() =>
                {
                    var devicePlus = new AudioOutputDevice(device);
                });*/

            /*try
            {*/
            /*UnityMainThreadDispatcher.Enqueue(() => {

            });*/

            /*Task.Run(() =>
            {
                try
                {
                    var devicePlus = new AudioOutputDevice(device);
                }
                catch (Exception ex)
                {
                    UnityMainThreadDispatcher.Enqueue(() => {
                        throw ex;
                    });
                }
            });*//*

            device.AudioClient.Initialize(
                AudioClientShareMode.Shared,
                AudioClientStreamFlags.None,
                10000000, // 1 секунда в наносекундах (пример)
                0,
                device.AudioClient.MixFormat,
                Guid.Empty
            );

            UnityEngine.Debug.Log($"{device.AudioClient.BufferSize}, {device.State}, {device.FriendlyName}");
            *//*}*//*
            try
            {
                var devicePlus = new AudioOutputDevice(device);
            }
            catch (COMException ex)
            {
                Debug.LogError($"COMException: {ex.Message}");
                Debug.LogError($"HRESULT (код ошибки): 0x{ex.ErrorCode:X}");
            }*/
        }

            //audioOutputsDictionary = outputDevices.ToDictionary(device => device.FriendlyName, device => new AudioOutputDevice(device));
            /*var enumerator = new MMDeviceEnumerator();
            var defaultInput = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
            var defaultOutput = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            var input = new AudioInputDevice(defaultInput);
            var output = new AudioOutputDevice(defaultOutput);

            StartIntercomStream(input, output);
            Thread.Sleep(5000);
            StopIntercomStream(input, output);*/
        //}


        public void PlayAudioClip(AudioOutputDevice device, AudioModule.AudioClip clipName, bool doCropPrevClip)
        {
            throw new System.NotImplementedException();
        }

        public void StartIntercomStream(AudioInputDevice input, AudioOutputDevice output)
        {
            var buffer = new InputStreamSampleProvider();
            input.DataAvailable += buffer.OnDataAvailable;
            output.AddSampleProvider(buffer);
            _intercomStreams.Add((input, output), buffer);
        }

        public void StopIntercomStream(AudioInputDevice input, AudioOutputDevice output)
        {
            var buffer = _intercomStreams[(input, output)];
            input.DataAvailable -= buffer.OnDataAvailable;
            output.RemoveSampleProvider(buffer);
            _intercomStreams.Remove((input, output));
        }
    }
}