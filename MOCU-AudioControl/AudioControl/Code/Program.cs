// dotnet publish -c Release

using AudioControl;
using InterprocessCommunication;

class Program
{
    public static void Main(string[] args)
    {
        new DaemonHandler_Server(
            commandProcessor: new AudioManager(),
            argsFromCaller: args
        ).StartDaemon();
    }
}