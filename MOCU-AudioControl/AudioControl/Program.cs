// dotnet publish -c Release

using System.Diagnostics;

using AudioControl;
using DeamonsNamespace.InterprocessCommunication;


class Program
{
    /// <summary>
    /// A global variable for storing the state of how the program was started: as a console application or as a hidden one.
    /// In case of incorrect program initialization -> "true" by default in order for errors to be accompanied by audio signals.
    /// </summary>
    public static bool isProcessHidden = true;

    static async Task Main(string[] args)
    {
        if (args.Length != 3)
            CloseProgram("Program / Main :: Not enough arguments were passed from Unity.\nShould be 3: (int: parentProcessId, string: namedPipeName, bool: isProcessHidden)");

        if (!bool.TryParse(args[2], out isProcessHidden))
            CloseProgram("Program / Main :: 'isProcessHidden' argument is not valid");

        if (!int.TryParse(args[0], out int parentProcessId))
            CloseProgram("Program / Main :: 'parentProcessId' argument is not valid");

        Console.WriteLine($"Program name: AudioControl");
        Console.WriteLine($"parentProcessId: {parentProcessId}");
        Console.WriteLine($"isProcessHidden: {isProcessHidden}\n");



        var namedPipeName = args[1];

        StartParentProcessMonitoring(parentProcessId, checkIntervalMs: 500);
        DebugFunc(doDebug: false, tickIntervalMs: 250);

        var audioManager = new AudioManager();
        var server = new NamedPipeServer(namedPipeName: namedPipeName, commandProcessor_dependencyInjection: audioManager.ProcessCommand);

        audioManager.outputMessagesQueue = server.outputMessagesQueue;  // must be before "server.StartAsync()"
        server.ErrorOccurred += ServerErrorOccurred;                    // the way to communicate: from server to Program
        // todo: maybe add later "server.innerMessagesQueue" to "audioManager.innerMessagesQueue"
        
        server.StartAsync();
    }

    
    

    /// <summary>
    /// Every "checkIntervalMs" milliseconds asks the OS if the parent process is still alive.
    /// If not -> closes the program. It is needed if Unity crashed and cannot close the external program correctly.
    /// </summary>
    private static void StartParentProcessMonitoring(int parentProcessId, int checkIntervalMs)
    {
        var monitorThread = new Thread(() =>
        {
            while (!Process.GetProcessById(parentProcessId).HasExited) {
                Thread.Sleep(checkIntervalMs);
            }

            // todo: in abstract class add as option
            CloseProgram("Program / Main :: Parent process died");
        });
        monitorThread.Start();
    }

    /// <summary>
    /// Produces a short beep every "tickIntervalMs" milliseconds for testing when it is not possible to output to the console.
    /// And for better human perception too.
    /// </summary>
    private static void DebugFunc(bool doDebug, int tickIntervalMs)
    {
        if (!doDebug) return;

        var debugThread = new Thread(() =>
        {
            while (true)
            {
                Console.Beep(800, 100);
                Thread.Sleep(tickIntervalMs);
            }
        });
        debugThread.Start();
    }

    /// <summary>
    /// Closes the program immediately. May incorrectly release resources
    /// </summary>
    public static void CloseProgram(string error)
    {
        if (isProcessHidden)
        {
            Console.Beep(1500, 500);
        }
        else
        {
            Console.WriteLine(error);
            Thread.Sleep(1000);         // give some time to see an error message
        }

        Environment.Exit(0);
    }

    private static void ServerErrorOccurred(string message)
    {
        CloseProgram(message);
    }
}