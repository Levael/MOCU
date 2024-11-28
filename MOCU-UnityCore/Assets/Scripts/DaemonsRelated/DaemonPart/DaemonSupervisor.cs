using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Reflection;
using System.Threading;


namespace DaemonsRelated
{
    public class DaemonSupervisor
    {
        private readonly string[] _arguments;

        private int _parentProcessId;
        private string _daemonName;
        private bool _isInDebugMode;

        private List<string> _terminationReasons;

        public DaemonSupervisor(string[] arguments)
        {
            _arguments = arguments;
            _terminationReasons = new List<string>();
        }

        public string DaemonName => _daemonName;

        public bool IsValid()
        {
            if (_arguments == null || _arguments.Length != 3)
            {
                _terminationReasons.Add("Insufficient _arguments provided");
                return false;
            }

            if (!bool.TryParse(_arguments[2], out var isHidden))
            {
                _terminationReasons.Add("Argument 'isProcessHidden' is not valid");
                return false;
            }

            if (!int.TryParse(_arguments[0], out _parentProcessId))
            {
                _terminationReasons.Add("Argument '_parentProcessId' is not valid");
                return false;
            }

            if (string.IsNullOrEmpty(_arguments[1]))
            {
                _terminationReasons.Add("Argument 'namedPipeName' is not valid");
                return false;
            }

            _daemonName = _arguments[1];
            _isInDebugMode = !isHidden;

            Console.WriteLine($"Is in debug mode: {_isInDebugMode}");

            return true;
        }

        public void SubscribeForParentProcessTermination()
        {
            var parentProcess = Process.GetProcessById(_parentProcessId);

            parentProcess.EnableRaisingEvents = true;
            parentProcess.Exited += (sender, e) =>
            {
                _terminationReasons.Add("Parent process terminated");
                CloseProgram();
            };
        }

        /*public void SubscribeForParentProcessTermination_alternative()
        {
            string query = $"SELECT * FROM Win32_ProcessStopTrace WHERE ProcessID = {_parentProcessId}";
            var watcher = new ManagementEventWatcher(new WqlEventQuery(query));
            watcher.EventArrived += new EventArrivedEventHandler((s, e) =>
            {
                _terminationReasons.Add("Parent process terminated");
                CloseProgram();
            });
            watcher.Start();
        }*/

        public void CloseProgram(string? reason = null)
        {
            if (reason != null)
                _terminationReasons.Add(reason);

            Console.WriteLine("Reasons for termination:");

            foreach (var terminationReason in _terminationReasons)
                Console.WriteLine($"\t{reason}");

            if (_isInDebugMode)
            {
                Console.WriteLine("Press any KEY to exit");
                Console.ReadKey();
            }

            Environment.Exit(1);
        }

        // temp here
        /*private static void PrintDebugInfo(string info)
        {
            StackTrace stackTrace = new StackTrace();

            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                StackFrame frame = stackTrace.GetFrame(i);
                MethodBase method = frame.GetMethod();

                Console.WriteLine($"{new string(' ', i * 2)}{method.DeclaringType}.{method.Name}: '{info}'");
            }
        }*/
    }
}