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
    private List<ExternalDaemon> _externalDaemonsList;

    void OnDestroy()
    {
        foreach (var daemon in _externalDaemonsList)
        {
            try { daemon.namedPipeClient?.Destroy(); } catch { }
            try { daemon.process?.Kill(); } catch { }
        }
    }


    /// <param name="executableFileName">executable daemon file name without '.exe'</param>
    /// <param name="isHidden">choose 'false' for debugging purposes</param>
    public async Task<ExternalDaemon> InitAndRunDaemon(string executableFileName, bool isHidden = true)
    {
        var daemon = new ExternalDaemon(executableFileName: executableFileName, isHidden: isHidden);

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

    
    public void StartDaemonProcess(ExternalDaemon daemon)
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

    public async Task ConnectToServer(ExternalDaemon daemon)
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

}

class ExternalDaemon
{
    public string executableFileName;
    public string namedPipeName;
    public bool isHidden;

    public bool processIsOk = false;
    public bool connectionIsOk = false;

    public NamedPipeClient namedPipeClient;
    public Process process;


    public ExternalDaemon(string executableFileName, bool isHidden = true)
    {
        this.executableFileName = executableFileName;
        this.namedPipeName = executableFileName;
        this.isHidden = isHidden;
    }
}

#nullable disable
#pragma warning restore CS8618