using DaemonsNamespace.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Reflection;


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
        }

        public string DaemonName => _daemonName;

        public bool IsValid()
        {
            if (_arguments == null || _arguments.Length != 3)
            {
                _terminationReasons.Add("Insufficient _arguments provided");
                return false;
            }

            if (!bool.TryParse(_arguments[2], out _isInDebugMode))
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

        public void SubscribeForParentProcessTermination_alternative()
        {
            string query = $"SELECT * FROM Win32_ProcessStopTrace WHERE ProcessID = {_parentProcessId}";
            var watcher = new ManagementEventWatcher(new WqlEventQuery(query));
            watcher.EventArrived += new EventArrivedEventHandler((s, e) =>
            {
                _terminationReasons.Add("Parent process terminated");
                CloseProgram();
            });
            watcher.Start();
        }

        public void CloseProgram()
        {
            Console.WriteLine("Reasons for termination:");

            foreach (var reason in _terminationReasons)
                Console.WriteLine($"\t{reason}");

            Console.ReadKey();
            Environment.Exit(1);


            //PrintDebugInfo(reason);
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