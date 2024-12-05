using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

using InterprocessCommunication;

using Debug = UnityEngine.Debug;
using System.Diagnostics;
using DaemonsRelated;
using System.Linq;

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
        daemon.MessageReceived += message => LogDaemonIncomingActivity(daemonName: daemonType, message: message);
        daemon.MessageSent += message => LogDaemonOutgoingActivity(daemonName: daemonType, message: message);
        daemon.Start();
        return daemon.GetCommunicator();
    }

    public int GetDaemonsNumber()
    {
        return _daemons.Count;
    }


    // todo: refactor later, not important for now

    private void LogDaemonIncomingActivity(DaemonType daemonName, string message, string messageName = "todo")
    {
        var errorReports = JsonHelper.DeserializeJson<MinimalDataTransferObject>(message).DaemonErrorReports;

        var messageType = DebugMessageType.Info;

        if (errorReports.Any())
            messageType = DebugMessageType.Warning;

        if (errorReports.Any(report => report.isFatal))
            messageType = DebugMessageType.Error;

        LogDaemonActivity(daemonName: daemonName, messageName: messageName, direction: MessageDirection.Incoming, messageType: messageType);
    }

    private void LogDaemonOutgoingActivity(DaemonType daemonName, string message, string messageName = "todo")
    {
        LogDaemonActivity(daemonName: daemonName, messageName: messageName, direction: MessageDirection.Outgoing);
    }

    private void LogDaemonActivity(DaemonType daemonName, string messageName, MessageDirection direction, DebugMessageType messageType = DebugMessageType.Info, string message = null)
    {
        _debugTabHandler.AddDaemonActivity(daemonName: $"{daemonName}", messageName: messageName, direction: direction, messageType: messageType);
    }
}

#nullable disable
#pragma warning restore CS8618