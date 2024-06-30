// dotnet publish -c Release

using AudioControl;

class Program
{
    public static void Main(string[] args)
    {
        new DaemonHandler_Server(
            daemonProgramManager: new AudioManager(),
            argsFromCaller: args
        ).Start();
    }
}