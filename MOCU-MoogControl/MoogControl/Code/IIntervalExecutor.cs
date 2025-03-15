using System;


namespace MoogModule.Daemon
{
    internal interface IIntervalExecutor
    {
        event Action OnTick;
        void ExecuteWithDelay(TimeSpan delay, Action action);
        void Start();
        void Stop();
        TimeSpan Interval { get; set; }
        TimeSpan AverageInterval { get; }
    }
}