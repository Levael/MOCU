using System;
using System.Collections.Generic;
using UnityEngine;

using AudioControl;
using UnityDaemonsCommon;


public partial class AudioHandler : MonoBehaviour
{
    public StateTracker stateTracker            { get; private set; }



    public List<string> GetDevicesList(DevicesListTypes type)
    {
        if (type == DevicesListTypes.Input)
            return inputAudioDevices;
        else if (type == DevicesListTypes.Output)
            return outputAudioDevices;
        else
            throw new Exception("DevicesListTypes is unknown or wrong");
    }
    public AudioDevicesInfo GetAudioDevicesInfo()
    {
        return audioDevicesInfo;
    }

    public string? GetAudioDeviceName(string fieldName)
    {
        return audioDevicesInfo.GetType().GetField(fieldName)?.GetValue(audioDevicesInfo).ToString();
    }
    public bool? SetAudioDeviceName(string fieldName, string deviceName)
    {
        if (String.IsNullOrEmpty(fieldName))
        {
            UnityEngine.Debug.LogError($"The device name is incorrect: {fieldName}");
            return false;
        }

        audioDevicesInfo.GetType().GetField(fieldName)?.SetValue(audioDevicesInfo, deviceName);
        return true;
    }
    public float? GetAudioDeviceVolume(string fieldName)
    {
        var answer = audioDevicesInfo.GetType().GetField(fieldName)?.GetValue(audioDevicesInfo);
        if (answer == null)
            return null;
        else
            return (float?)answer;
    }
    public bool? SetAudioDeviceVolume(string fieldName, float? deviceVolume)
    {
        if (String.IsNullOrEmpty(fieldName))
        {
            UnityEngine.Debug.LogError($"The device name is incorrect: {fieldName}");
            return false;
        }

        var linkToVolume = audioDevicesInfo.GetType().GetField(fieldName);

        if (deviceVolume < 0f || deviceVolume > 100f || deviceVolume == null)
        {
            UnityEngine.Debug.LogError($"The device volume is incorrect: {deviceVolume}");
            return false;
        }

        if ((float)linkToVolume.GetValue(audioDevicesInfo) == deviceVolume)
        {
            UnityEngine.Debug.LogWarning($"Same volume");
            return false;
        }

        linkToVolume.SetValue(audioDevicesInfo, deviceVolume);
        return true;
    }

    public void SendTestAudioSignalToDevice(string audioOutputDeviceName, string audioFileName = "test.mp3")    // todo: move 'audioFileName' to config
    {
        namedPipeClient.SendCommandAsync(CommonUtilities.SerializeJson(new PlayAudioFile_Command(audioFileName: audioFileName, audioOutputDeviceName: audioOutputDeviceName)));
    }

}