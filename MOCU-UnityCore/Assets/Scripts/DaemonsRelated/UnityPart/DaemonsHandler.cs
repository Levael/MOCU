using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

using InterprocessCommunication;

using Debug = UnityEngine.Debug;
using System.Diagnostics;
using DaemonsRelated;

#nullable enable
#pragma warning disable CS8618


class DaemonsHandler : MonoBehaviour, IControllableInitiation
{
    public bool IsComponentReady { get; private set; }

    private string _daemonsFolderPath;
    private Dictionary<DaemonType, (string fullPath, bool isHidden, IDaemonUser businessLogic)> _daemonControlPaths;
    private Dictionary<DaemonType, HostSideDaemonContainer> _daemons;
    private List<DaemonHandler_Client> _daemonHandlers;

    private DebugTabHandler _debugTabHandler;

    public void ControllableAwake()
    {
        _debugTabHandler = GetComponent<DebugTabHandler>();

        _daemonHandlers = new();
        _daemonsFolderPath = Path.Combine(Application.streamingAssetsPath, "Daemons");

        _daemons = new()
        {
            { 
                DaemonType.Audio, new HostSideDaemonContainer(
                    type: DaemonType.Audio,
                    fullPath: Path.Combine(_daemonsFolderPath, "AudioControl.exe"),
                    isHidden: false
                )
            }
        };
    }

    public void ControllableStart()
    {
        IsComponentReady = true;
    }

    void OnApplicationQuit()
    {
        foreach (var daemon in _daemons.Values)
            try { daemon.Stop(); } catch { }
    }


    public IInterprocessCommunicator GetDaemonCommunicator(DaemonType daemonType)
    {
        var daemon = _daemons[daemonType];
        daemon.MessageReceived += message => LogDaemonIncomingActivity(daemonType);
        daemon.MessageSent += message => LogDaemonOutgoingActivity(daemonType);
        daemon.Start();
        return daemon.GetCommunicator();
    }



    /*public async Task<DaemonHandler_Client> GenerateDaemon(DaemonType daemonControlsEnum)
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
            Debug.LogError($"couldn't execute 'GenerateDaemon' properly for {daemonControlsEnum} daemon. Exception: {ex}");
        }

        return daemon;
    }*/

    public int GetDaemonsNumber()
    {
        return _daemons.Count;
    }


    // todo: refactor later, not important for now

    private void LogDaemonIncomingActivity(DaemonType daemonName, string messageName = "todo")
    {
        LogDaemonActivity(daemonName: daemonName, messageName: messageName, direction: MessageDirection.Incoming);
    }

    private void LogDaemonOutgoingActivity(DaemonType daemonName, string messageName = "todo")
    {
        LogDaemonActivity(daemonName: daemonName, messageName: messageName, direction: MessageDirection.Outgoing);
    }

    private void LogDaemonActivity(DaemonType daemonName, string messageName, MessageDirection direction)
    {
        _debugTabHandler.AddDaemonActivity(daemonName: $"{daemonName}", messageName: messageName, direction: direction);
    }
}

#nullable disable
#pragma warning restore CS8618