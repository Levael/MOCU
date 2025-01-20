using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using AudioModule;
using InterprocessCommunication;
using DaemonsRelated;


public class AudioHandler_new : MonoBehaviour, AudioHandler_API, IControllableInitiation
{
    public bool IsComponentReady { get; private set; }

    public event Action DeviceHasChanged;
    public event Action ClipHasChanged;
    public event Action IntercomHasStarted;
    public event Action IntercomHasStoped;

    private AudioHostSideBridge _daemon;
    private Dictionary<Guid, (AudioDeviceData, User)> _devices;
    private Dictionary<AudioClipName, AudioClipData> _clips;
    private Dictionary<(Guid fromDevice, Guid toDevice), AudioIntercomData> _intercoms;

    // unity components
    private DaemonsHandler _daemonsHandler;
    private DebugTabHandler _debugTabHandler;

    // ############################################################################################

    public void ControllableAwake()
    {
        _daemonsHandler = GetComponent<DaemonsHandler>();
        _debugTabHandler = GetComponent<DebugTabHandler>();
    }
    public void ControllableStart()
    {
        _daemon = new AudioHostSideBridge(_daemonsHandler.GetDaemonCommunicator(DaemonType.Audio));

        InitDaemonTest();

        _debugTabHandler.testBtn1Clicked += (eventObj) => _daemon.TestMethod1();
        _debugTabHandler.testBtn2Clicked += (eventObj) => _daemon.TestMethod2();
    }

    // ############################################################################################

    public void ChangeClipParameters(AudioClipData clipData)
    {
        throw new NotImplementedException();
    }

    public void ChangeDeviceUser(Guid deviceId, User user)
    {
        throw new NotImplementedException();
    }

    public void ChangeDeviceVolume(Guid deviceId, float volume)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<AudioClipData> GetClips()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<(AudioDeviceData deviceData, User deviceUser)> GetInputDevices()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<AudioIntercomData> GetIntercoms()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<(AudioDeviceData deviceData, User deviceUser)> GetOutputDevices()
    {
        throw new NotImplementedException();
    }

    public void PingDevice(Guid deviceId)
    {
        throw new NotImplementedException();
    }

    public void PlayClip(User user, AudioClipName clip)
    {
        throw new NotImplementedException();
    }

    public void StartIntercom(User fromUser, User toUser)
    {
        throw new NotImplementedException();
    }

    public void StopIntercom(User fromUser, User toUser)
    {
        throw new NotImplementedException();
    }

    // ############################################################################################

    private void InitDaemonTest()
    {
        _daemon.UpdateClipsData(
            new List<AudioClipData>() {
                new AudioClipData() {
                    name = AudioClipName.PingDevice,
                    volume = 100,
                    fullFilePath = @"C:\Users\Levael\GitHub\MOCU\MOCU-UnityCore\Assets\StreamingAssets\Audio\test.mp3"
                },

                new AudioClipData() {
                    name = AudioClipName.CorrectAnswer,
                    volume = 85,
                    fullFilePath = @"C:\Users\Levael\GitHub\MOCU\MOCU-UnityCore\Assets\StreamingAssets\Audio\audioTestSample.mp3"
                }
            }
        );
    }
}