using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

using InterprocessCommunication;

#nullable enable
#pragma warning disable CS8618


class DaemonsHandler : MonoBehaviour
{
    private List<DaemonProcess> _externalDaemonsList;

    void OnDestroy()
    {
        foreach (var daemon in _externalDaemonsList)
        {
            KillDaemon(daemon);
        }
    }



    /// <param name="executableFileName">executable daemon file name without '.exe'</param>
    /// <param name="isHidden">choose 'false' for debugging purposes</param>
    public async Task<DaemonProcess> InitAndRunDaemon(string executableFileName, bool isHidden = true)
    {
        var daemon = new DaemonProcess(executableFileName: executableFileName, isHidden: isHidden);

        try
        {
            StartDaemonProcess(daemon);
            await ConnectToServer(daemon);
        }
        catch
        {
            UnityEngine.Debug.LogError($"couldn't execute 'InitAndRunDaemon' properly for {executableFileName} daemon");
        }

        return daemon;
    }

    
    public void StartDaemonProcess(DaemonProcess daemon)
    {
        try
        {
            string fullExternalAppPath = Path.Combine(Application.streamingAssetsPath, $"{daemon.executableFileName}.exe");

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = fullExternalAppPath,
                Arguments = $"{Process.GetCurrentProcess().Id} {daemon.namedPipeName} {daemon.isHidden}", // takes string where spaces separate arguments
                UseShellExecute = !daemon.isHidden,
                RedirectStandardOutput = false,
                CreateNoWindow = daemon.isHidden
            };

            daemon.process = new Process() { StartInfo = startInfo };
            daemon.process.Start();

            daemon.processIsOk = true;
        }
        catch
        {
            daemon.processIsOk = false;
            throw new Exception();
        }
    }

    public async Task ConnectToServer(DaemonProcess daemon)
    {
        try
        {
            var namedPipeClient = new NamedPipeClient(daemon.namedPipeName);
            daemon.connectionIsOk = await namedPipeClient.StartAsync();
        }
        catch
        {
            daemon.connectionIsOk = false;
            throw new Exception();
        }
    }

    public void KillDaemon(DaemonProcess daemon)
    {
        try { daemon.namedPipeClient?.Destroy();    } catch { }
        try { daemon.process?.Kill();               } catch { }
    }
}

#nullable disable
#pragma warning restore CS8618