using System;
using System.Collections.Generic;
using UnityEngine;

using AudioControl;
using UnityDaemonsCommon;
using InterprocessCommunication;

using Debug = UnityEngine.Debug;


public partial class AudioHandler : MonoBehaviour
{
    public StateTracker stateTracker { get; private set; }


    /// <summary>
    /// Returns data that client has (since last update).
    /// May not be the most relevant at the moment. To get most updated one -- call 'RequestAudioDataUpdate' and then 'GetAudioData').
    /// In case of UI (to get ost apdated data) enough to call 'RequestAudioDataUpdate'. UI update method will be called automatically.
    /// </summary>
    public UnifiedAudioDataPacket GetAudioData()
    {
        return new UnifiedAudioDataPacket(
            audioDevicesInfo : audioDevicesInfo,
            inputAudioDevices : inputAudioDevices,
            outputAudioDevices : outputAudioDevices
        );
    }


    /// <summary>
    /// Instead of single changes, it sends the entire 'AudioDevicesInfo' updated object at once
    /// </summary>
    public void SetAudioData(AudioDevicesInfo audioDevicesInfo)
    {
        SendClientAudioDataDesire(audioDevicesInfo);
    }

    public void SendTestAudioSignalToDevice(string audioOutputDeviceName, string audioFileName = "test.mp3")    // todo: move 'audioFileName' to config
    {
        if (_daemon == null || !_daemon.isConnectionOk || !_daemon.isProcessOk)
        {
            Debug.LogError("Custom: 'SendTestAudioSignalToDevice' is unavailable right now");
            return;
        }
        _daemon.SendCommand(new UnifiedCommandFrom_Client(name: "PlayAudioFile_Command", extraData: new PlayAudioFile_CommandDetails(audioFileName: audioFileName, audioOutputDeviceName: audioOutputDeviceName)));
    }
    public void PlayAudioClip(string clipName, string deviceName) { }
}

