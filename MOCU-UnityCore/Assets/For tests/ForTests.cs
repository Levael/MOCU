using System;
using System.Linq;
using System.Text;
using Unity.Profiling;
using UnityEngine;

public class ForTests : MonoBehaviour
{
    private DebugTabHandler _debugTabHandler;
    private AudioHandler _audioHandler;
    private CedrusHandler _cerusHandler;

    //private int temp_counter = 0;

    private void Awake()
    {
        _debugTabHandler = GetComponent<DebugTabHandler>();
        _audioHandler = GetComponent<AudioHandler>();
        _cerusHandler = GetComponent<CedrusHandler>();
    }

    private void Start()
    {
        _debugTabHandler.testBtn1Clicked += (eventObj) =>
        {
            if (_cerusHandler.stateTracker.Status != DeviceConnection_Statuses.Connected)
                _cerusHandler.TryConnect();
        };

        // sending lots of commands simultaneously together (stresstest)
        /*_debugTabHandler.testBtn1Clicked += (eventObj) =>
        {
            print($"sended");
            _audioHandler.
            //_audioHandler.SendTestAudioSignalToDevice("Speakers (Realtek High Definition Audio)");
        };*/

    }

    private void Update()
    {
        /*if (_audioHandler.stateTracker.Status == DeviceConnection_Statuses.Connected && temp_counter++ < 300)
            _audioHandler.SendTestAudioSignalToDevice("Speakers (Realtek High Definition Audio)");*/
    }
}
