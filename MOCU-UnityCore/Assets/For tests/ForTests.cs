using UnityEngine;

using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using AudioModule_NAudio;



public class ForTests : MonoBehaviour, IFullyControllable
{
    public bool IsComponentReady {  get; private set; }

    private DebugTabHandler _debugTabHandler;
    private AudioHandler _audioHandler;
    //private CedrusHandler _cerusHandler;
    private ExperimentTabHandler _experimentTabHandler;

    //private int temp_counter = 0;

    public void ControllableAwake()
    {
        _debugTabHandler = GetComponent<DebugTabHandler>();
        _audioHandler = GetComponent<AudioHandler>();
        _experimentTabHandler = GetComponent<ExperimentTabHandler>();

        
        //_cerusHandler = GetComponent<CedrusHandler>();
    }

    public void ControllableStart()
    {
        _debugTabHandler.testBtn1Clicked += (eventObj) =>
        {
            try
            {
                var audio = new AudioBackend();
                audio.Test();
            }
            catch (Exception ex)
            {
                Debug.LogError($": {ex}");
                //_experimentTabHandler.PrintToInfo($"{ex}");
            }
            //_experimentTabHandler.PrintToInfo($"{DateTime.Now}\n{TestMethod()}", true);
            /*if (_cerusHandler.stateTracker.Status != ModuleStatus.FullyOperational)
                _cerusHandler.TryConnect();*/
        };

        
        //GetComponent<AudioSource>().Test();

        // sending lots of commands simultaneously together (stresstest)
        /*_debugTabHandler.testBtn1Clicked += (eventObj) =>
        {
            print($"sended");
            _audioHandler.
            //_audioHandler.SendTestAudioSignalToDevice("Speakers (Realtek High Definition Audio)");
        };*/
        IsComponentReady = true;
    }

    public void ControllableUpdate()
    {
        /*if (_audioHandler.stateTracker.Status == ModuleStatus.FullyOperational && temp_counter++ < 300)
            _audioHandler.SendTestAudioSignalToDevice("Speakers (Realtek High Definition Audio)");*/
    }


    private string TestMethod()
    {
        var enumerator = new MMDeviceEnumerator();
        var unifiedWaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
        var mixer = new MixingSampleProvider(unifiedWaveFormat);
        var captureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
        var deviceNames = new List<string>();

        foreach (var device in captureDevices)
            deviceNames.Add(device.FriendlyName);

        deviceNames.Add(unifiedWaveFormat.SampleRate.ToString());
        deviceNames.Add(mixer.MixerInputs.ToString());

        var json = JsonConvert.SerializeObject(deviceNames, Formatting.Indented);

        return json;
    }
}
