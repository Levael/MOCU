using System;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Diagnostics;
using System.Management;
using System.Collections.Generic;


namespace MoogModule.Daemon
{
    public static class SystemOptimizer
    {
        // todo (temp): change later to parameter

        private static int AllowedCoresMask;
        private static int BlockedCoresMask; // Complementary to AllowedCoresMask
        private static readonly Dictionary<int, IntPtr> originalAffinity = new();
        private static ManagementEventWatcher _watcher;

        public static void Optimize(int reserveCores = 2)
        {
            int totalCores = Environment.ProcessorCount;

            if (reserveCores > totalCores)
                reserveCores = totalCores;

            AllowedCoresMask = (1 << reserveCores) - 1; // for 2 it will be 0b00000011
            BlockedCoresMask = ~AllowedCoresMask;

            AnnexCores();
            ExpelOthersFromAnnexedCores();
            WatchNewProcesses();

            // ---

            TimeBeginPeriod(1);
            SetHighPriority();
        }

        public static void ResetToDefault()
        {
            TimeEndPeriod(1);
            StopWatchingProcesses();
            RestoreOriginalAffinity();
        }

        private static void AnnexCores()
        {
            try
            {
                Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)AllowedCoresMask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Couldn't 'AnnexCores' cores");
            }
        }

        private static void ExpelOthersFromAnnexedCores()
        {
            foreach (Process process in Process.GetProcesses())
            {
                try
                {
                    ExpelFromAnnexedCores(process);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Couldn't 'ExpelOthersFromAnnexedCores' cores: {process.ProcessName}");
                }
            }
        }

        private static void ExpelFromAnnexedCores(Process process)
        {
            try
            {
                if (process.Id == Process.GetCurrentProcess().Id) return;

                IntPtr affinity = process.ProcessorAffinity;
                originalAffinity[process.Id] = affinity;
                process.ProcessorAffinity = (IntPtr)(affinity.ToInt64() & BlockedCoresMask);
                Console.WriteLine($"Expelled successfully: {process.ProcessName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Couldn't 'ExpelFromAnnexedCores' cores");
            }
        }

        private static void RestoreOriginalAffinity()
        {
            foreach (var kvp in originalAffinity)
            {
                try
                {
                    var process = Process.GetProcessById(kvp.Key);
                    process.ProcessorAffinity = kvp.Value;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Couldn't 'RestoreOriginalAffinity' {kvp.Key}. Maybe it was already terminated");
                }
            }
        }

        private static void WatchNewProcesses()
        {
            try
            {
                _watcher = new ManagementEventWatcher("SELECT * FROM Win32_ProcessStartTrace");
                _watcher.EventArrived += OnProcessStarted;
                _watcher.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Couldn't handle an error in 'WatchNewProcesses'");
            }
        }

        private static void OnProcessStarted(object sender, EventArrivedEventArgs e)
        {
            string processName = e.NewEvent["ProcessName"].ToString();
            int processId = Convert.ToInt32(e.NewEvent["ProcessID"]);

            try
            {
                var process = Process.GetProcessById(processId);
                ExpelFromAnnexedCores(process);
            }
            catch {}
        }

        private static void StopWatchingProcesses()
        {
            if (_watcher != null)
            {
                _watcher.EventArrived -= OnProcessStarted;
                _watcher.Stop();
                _watcher.Dispose();
                _watcher = null;
            }
        }

        private static void SetHighPriority()
        {
            var process = Process.GetCurrentProcess();
            process.PriorityClass = ProcessPriorityClass.High;

            foreach (ProcessThread thread in process.Threads)
            {
                try { thread.PriorityLevel = ThreadPriorityLevel.Highest; }
                catch { }
            }
        }

        public static void PauseGarbageCollector()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;
        }

        public static void ResumeGarbageCollector()
        {
            GCSettings.LatencyMode = GCLatencyMode.Interactive;
        }


        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
        public static extern void TimeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
        public static extern void TimeEndPeriod(uint uMilliseconds);
    }
}