using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

using InterprocessCommunication;

#nullable enable
#pragma warning disable CS8618


class DaemonsHandler : MonoBehaviour
{
    public enum Daemons
    {
        Audio,
        Moog,
        EEG,
        Video
    }

    private Dictionary<Daemons, (string fullPath, bool isHidden, IDaemonUser businessLogic)> _daemonControlPaths;
    private List<DaemonHandler_Client> _daemonHandlers = new();


    void Awake()
    {
        _daemonControlPaths = new()
        {
            { Daemons.Audio, (
                fullPath: Path.Combine(Application.streamingAssetsPath, "Daemons/AudioControl.exe"),
                isHidden: false,
                businessLogic: GetComponent<AudioHandler>()
            )},
        };
    }

    void OnDestroy()
    {
        foreach (var daemon in _daemonHandlers)
            try { daemon.StopDaemon(); } catch { }
    }



    public async Task<DaemonHandler_Client> CreateDaemon(Daemons daemonControlsEnum)
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
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"couldn't execute 'CreateDaemon' properly for {daemonControlsEnum} daemon. Exception: {ex}");
        }

        return daemon;
    }
}

#nullable disable
#pragma warning restore CS8618