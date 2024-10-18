using DaemonsNamespace.Common;
using System;
using System.Diagnostics;
using System.Management;
using System.Reflection;


namespace DaemonsRelated
{
    public class DaemonLauncher
    {
        public void Main(string[] arguments)
        {
            ValidateArguments(arguments);

            var parentProcessId = int.Parse(arguments[0]);
            var daemonName = arguments[1];
            var isInDebugMode = bool.Parse(arguments[2]);

            DaemonUtilities.ConsoleInfo($"Daemon name: {arguments[1]}");
            DaemonUtilities.ConsoleInfo($"Parent process id: {parentProcessId}\n");

            SubscribeForParentProcessTermination(parentProcessId);
            Console.ReadKey();
        }



        private void ValidateArguments(string[] arguments)
        {
            if (arguments == null || arguments.Length != 3)
                CloseProgram("Insufficient arguments provided");

            if (!bool.TryParse(arguments[2], out _))
                CloseProgram("Argument 'isProcessHidden' is not valid");

            if (!int.TryParse(arguments[0], out _))
                CloseProgram("Argument '_parentProcessId' is not valid");

            if (string.IsNullOrEmpty(arguments[1]))
                CloseProgram("Argument 'namedPipeName' is not valid");
        }

        private void SubscribeForParentProcessTermination(int parentProcessId)
        {
            var parentProcess = Process.GetProcessById(parentProcessId);

            parentProcess.EnableRaisingEvents = true;
            parentProcess.Exited += (sender, e) =>
            {
                CloseProgram("Parent process terminated");
            };
        }

        private void SubscribeForParentProcessTermination_alternative(int parentProcessId)
        {
            string query = $"SELECT * FROM Win32_ProcessStopTrace WHERE ProcessID = {parentProcessId}";
            var watcher = new ManagementEventWatcher(new WqlEventQuery(query));
            watcher.EventArrived += new EventArrivedEventHandler((s, e) => { CloseProgram("Parent process terminated"); });
            watcher.Start();
        }

        private void CloseProgram(string reason)
        {
            PrintDebugInfo(reason);

            Console.ReadKey();
            Environment.Exit(1);
            //StopDaemon();

            //if (!_isProcessHidden)
            //    Console.ReadKey();

            //Environment.Exit(1);
        }

        // temp here
        private static void PrintDebugInfo(string info)
        {
            StackTrace stackTrace = new StackTrace();

            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                StackFrame frame = stackTrace.GetFrame(i);
                MethodBase method = frame.GetMethod();

                Console.WriteLine($"{new string(' ', i * 2)}{method.DeclaringType}.{method.Name}: '{info}'");
            }
        }
    }
}
