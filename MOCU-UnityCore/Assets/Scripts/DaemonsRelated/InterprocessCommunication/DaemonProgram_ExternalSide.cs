using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using DaemonsNamespace.Common;
using CustomDataStructures;


namespace DaemonsNamespace.InterprocessCommunication
{
    public class DaemonProgram<DPM> where DPM : AbstractDaemonProgramManager
    {
        private bool isProcessHidden = true;
        private int parentProcessId;
        private string namedPipeName;
        private DPM daemonProgramManager;
        private NamedPipeServer communicationServer;



        /// <param name="argsFromCaller">Should be 3: (int: parentProcessId, string: namedPipeName, bool: isProcessHidden)</param>
        public DaemonProgram(DPM daemonProgramManager, string[] argsFromCaller) 
        {
            if (argsFromCaller == null || argsFromCaller.Length != 3)
                CloseProgram("Program / Main :: Insufficient arguments provided");

            if (!bool.TryParse(argsFromCaller[2], out isProcessHidden))
                CloseProgram("Program / Main :: 'isProcessHidden' argument is not valid");

            if (!int.TryParse(argsFromCaller[0], out parentProcessId))
                CloseProgram("Program / Main :: 'parentProcessId' argument is not valid");

            if (string.IsNullOrEmpty(argsFromCaller[1]))
                CloseProgram("Program / Main :: 'namedPipeName' argument is not valid");

            namedPipeName = argsFromCaller[1];

            //this.daemonProgram = daemonProgram;
            this.daemonProgramManager = daemonProgramManager;

            DaemonsUtilities.ConsoleInfo($"Program name: {daemonProgramManager.GetType()}");
            DaemonsUtilities.ConsoleInfo($"Parent process id: {parentProcessId}\n");
        }

        public void RunDaemon()
        {
            StartParentProcessMonitoring(parentProcessId, checkIntervalMs: 500);

            communicationServer = new (namedPipeName: namedPipeName, commandProcessor: daemonProgramManager.ProcessCommand);
            communicationServer.ErrorOccurred += ServerErrorOccurred;   // the way to communicate: from server to Program

            daemonProgramManager.outputMessagesQueue = communicationServer.outputMessagesQueue; // must be before "server.StartAsync()"

            communicationServer.StartAsync();
        }



        /// <summary>
        /// Closes the program immediately but leaves the console window open with an error message if 'isProcessHidden' is false (waits for key press).
        /// It's not called if the current process is killed from outside
        /// </summary>
        /// <param name="error">The error message to display in the console before closing.</param>
        private void CloseProgram(string error)
        {
            if (isProcessHidden || String.IsNullOrEmpty(error))
            {
                // Optional beep or any other action when the process is hidden
                //Console.Beep(1500, 500);
            }
            else
            {
                DaemonsUtilities.ConsoleError(error);
                Console.ReadKey();
            }

            Environment.Exit(0);
        }

        /// <summary>
        /// Every "checkIntervalMs" milliseconds asks the OS if the parent process is still alive.
        /// If not -> closes the program. It is needed if Unity crashed and cannot close the external program correctly.
        /// </summary>
        private void StartParentProcessMonitoring(int parentProcessId, int checkIntervalMs)
        {
            var monitorThread = new Thread(() =>
            {
                while (!Process.GetProcessById(parentProcessId).HasExited)
                {
                    Thread.Sleep(checkIntervalMs);
                }

                CloseProgram("Program / Main :: Parent process died");
            });
            monitorThread.Start();
        }

        private void ServerErrorOccurred(string message)
        {
            CloseProgram($"ServerErrorOccurred: {message}");
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
    }

}