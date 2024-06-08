using System.Diagnostics;

using InterprocessCommunication;


class DaemonProcess
{
    public string executableFileName;
    public string namedPipeName;
    public bool isHidden;

    public bool processIsOk = false;
    public bool connectionIsOk = false;

    public NamedPipeClient namedPipeClient;
    public Process process;


    public DaemonProcess(string executableFileName, bool isHidden = true)
    {
        this.executableFileName = executableFileName;
        this.namedPipeName = executableFileName;
        this.isHidden = isHidden;
    }
}