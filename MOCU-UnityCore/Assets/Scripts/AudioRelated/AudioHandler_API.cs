using System;
using System.Collections.Generic;
using UnityEngine;

using AudioControl;
using UnityDaemonsCommon;


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
    /// Sends request to the server. Doesn't get the response, it's done using other logic.
    /// </summary>
    public void RequestAudioDataUpdate()
    {
        //RequestAudioDevices();
        //_daemon.namedPipeClient.SendCommandAsync(CommonUtilities.SerializeJson(new GetAudioDevices_Command(doUpdate: true)));
    }

    /// <summary>
    /// Instead of single changes, it sends the entire 'AudioDevicesInfo' updated object at once
    /// </summary>
    public void SetAudioData(AudioDevicesInfo audioDevicesInfo) {}

    public void SendTestAudioSignalToDevice(string audioOutputDeviceName, string audioFileName = "test.mp3")    // todo: move 'audioFileName' to config
    {
        _daemon.namedPipeClient.SendCommandAsync(CommonUtilities.SerializeJson(new PlayAudioFile_Command(audioFileName: audioFileName, audioOutputDeviceName: audioOutputDeviceName)));
    }
    public void PlayAudioClip(string clipName, string deviceName) { }
}

