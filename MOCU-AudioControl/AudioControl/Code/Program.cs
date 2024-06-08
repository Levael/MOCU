// dotnet publish -c Release

using AudioControl;
using DaemonsNamespace.InterprocessCommunication;

// This is how daemon should look like: initiation and start. All logic in 'daemonProgramManager' and all minimal params are from 'args'

class Program
{
    public static void Main(string[] args)
    {
        new DaemonProgram<AudioManager>(
            daemonProgramManager: new AudioManager(),
            argsFromCaller: args
        ).RunDaemon();
    }
}