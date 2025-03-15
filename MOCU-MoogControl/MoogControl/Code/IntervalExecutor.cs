using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;


/*
 * About the class:
 * 
 * Tries to tick every 'TimeSpan' value passed in constructor or set via the 'Interval' property
 * ------------------------------------------------------------------------------------------------
 * About _intervalErrorRatio:
 * 
 * Ostensibly, the number of ticks between each check is whether it's time to tick,
 * but in fact, empty operations are performed faster.
 * As a result, just pick it up manually.
 * ------------------------------------------------------------------------------------------------
 * About _windowLengthForAverageInterval:
 * 
 * Uses the Incremental Mean algorithm.
 * The '_windowLengthForAverageInterval' value is manually chosen to be relatively stable while still being responsive.
 * Function: Mn = Ms + (Xn - Ms / N)
 * Mn = updated mean
 * Ms = old mean
 * Xn = newest element
 * N = observed number of elements
 */


namespace MoogModule.Daemon
{
    internal class IntervalExecutor : IIntervalExecutor
    {
        [DllImport("kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);


        private TimeSpan _interval;
        private double _intervalErrorRatio;
        private readonly ConcurrentQueue<Action> _postponedActions;
        private Thread _tickerThread;
        private volatile bool _isTicking;
        private int _ticksToSpinWait;
        public long _averageIntervalTicks;
        private long _windowLengthForAverageInterval;
        private long _queryPerformanceFrequency;

        public IntervalExecutor(TimeSpan interval)
        {
            _interval = interval;
            _postponedActions = new();
            _tickerThread = new Thread(Tick);
            _isTicking = false;
            _intervalErrorRatio = 0.05;  // 5% of _interval.Ticks (defines _ticksToSpinWait)
            
            _averageIntervalTicks = _interval.Ticks;   // serves only as first value

            _ticksToSpinWait = (int)(_interval.Ticks * _intervalErrorRatio);
            _ticksToSpinWait = Math.Max(_ticksToSpinWait, 1); // at least 1 iteration

            QueryPerformanceFrequency(out _queryPerformanceFrequency);   // puts value into '_queryPerformanceFrequency'
            _windowLengthForAverageInterval = _queryPerformanceFrequency * 60;    // 1 min

            Console.WriteLine($"QueryPerformanceFrequency: {_queryPerformanceFrequency} Hz");
            Console.WriteLine($"TimeSpan.Ticks per second: {TimeSpan.TicksPerSecond} Hz");
        }

        public event Action? OnTick;
        public TimeSpan Interval { get => _interval; set => throw new NotImplementedException(); }
        public TimeSpan AverageInterval => TimeSpan.FromSeconds((double)_averageIntervalTicks / _queryPerformanceFrequency);



        public void ExecuteWithDelay(TimeSpan delay, Action action)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            /*_tickerThread.Priority = ThreadPriority.AboveNormal; // Или Highest
            Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)1;
            SystemOptimizer.SetThreadExecutionState();*/
            //SystemOptimizer.TimeBeginPeriod(1);

            _isTicking = true;
            _tickerThread.Start();
        }

        public void Stop()
        {
            //SystemOptimizer.TimeEndPeriod(1);
            _isTicking = false;
        }

        private void Tick()
        {
            long now;
            long target;

            while (_isTicking)
            {
                try
                {
                    QueryPerformanceCounter(out now);
                    target = now + _interval.Ticks;

                    OnTick?.Invoke();

                    while (true)
                    {
                        QueryPerformanceCounter(out now);
                        if (now >= target) break;
                        //if (now >= (target - (_averageIntervalTicks - _interval.Ticks))) break; // additional "minus" helps reduce delta from +256 to +128
                        Thread.SpinWait(_ticksToSpinWait);
                        //Thread.Sleep(1);    // for test
                    }
                    //Console.WriteLine(now - target);

                    // updates Incremental Mean
                    // (now - target + _interval.Ticks) = length in ticks of the last loop
                    _averageIntervalTicks += ((now - target + _interval.Ticks) - _averageIntervalTicks) / _windowLengthForAverageInterval;

                    if ((now - target + _interval.Ticks) > (_interval.Ticks * 5))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"excedded 5ms!: {(double)(now - target + _interval.Ticks) / _interval.Ticks}ms");
                        Console.ResetColor();
                    }
                    //Console.WriteLine(((now - target) / _queryPerformanceFrequency));    // ms
                    //if ((now - target) != 0) 
                        //Console.WriteLine(now - target);    // ms
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"error handled. occurred in 'Tick' method: {ex}");
                }
            }            
        }
    }
}