using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

using InterprocessCommunication;

using Debug = UnityEngine.Debug;

#nullable enable
#pragma warning disable CS8618


class DaemonsHandler : MonoBehaviour, IControllableInitiation
{
    public enum DaemonType
    {
        Audio,
        Moog,
        EEG,
        Video
    }

    private Dictionary<DaemonType, (string fullPath, bool isHidden, IDaemonUser businessLogic)> _daemonControlPaths;
    private List<DaemonHandler_Client> _daemonHandlers = new();

    private DebugTabHandler _debugTabHandler;

    public bool IsComponentReady {  get; private set; }


    public void ControllableAwake()
    {
        _debugTabHandler = GetComponent<DebugTabHandler>();


        _daemonControlPaths = new()
        {
            { DaemonType.Audio, (
                fullPath: Path.Combine(Application.streamingAssetsPath, "DaemonType/AudioControl.exe"),
                isHidden: false,
                businessLogic: GetComponent<AudioHandler>()
            )},
        };
    }

    public void ControllableStart()
    {
        IsComponentReady = true;
    }

    void OnDestroy()
    {
        foreach (var daemon in _daemonHandlers)
            try { daemon.StopDaemon(); } catch { }
    }



    public async Task<DaemonHandler_Client> CreateDaemon(DaemonType daemonControlsEnum)
    {
        var daemon = new DaemonHandler_Client(
            exeFullFilePath: _daemonControlPaths[daemonControlsEnum].fullPath,
            isDaemonHidden: _daemonControlPaths[daemonControlsEnum].isHidden,
            businessLogic: _daemonControlPaths[daemonControlsEnum].businessLogic
        );

        try
        {
            await daemon.StartDaemon();
            _daemonHandlers.Add(daemon);

            daemon.MessageSended += LogDaemonOutgoingActivity;
            daemon.MessageReceived += LogDaemonIncomingActivity;
        }
        catch (Exception ex)
        {
            Debug.LogError($"couldn't execute 'CreateDaemon' properly for {daemonControlsEnum} daemon. Exception: {ex}");
        }

        return daemon;
    }

    public int GetDaemonsNumber()
    {
        return _daemonControlPaths.Count;
    }




    private void LogDaemonIncomingActivity(string daemonName, string messageName)
    {
        LogDaemonActivity(daemonName: daemonName, messageName: messageName, direction: MessageDirection.Incoming);
    }

    private void LogDaemonOutgoingActivity(string daemonName, string messageName)
    {
        LogDaemonActivity(daemonName: daemonName, messageName: messageName, direction: MessageDirection.Outgoing);
    }

    private void LogDaemonActivity(string daemonName, string messageName, MessageDirection direction)
    {
        _debugTabHandler.AddDaemonActivity(daemonName: daemonName, messageName: messageName, direction: direction);
    }
}

#nullable disable
#pragma warning restore CS8618