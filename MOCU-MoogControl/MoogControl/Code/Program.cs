using System;


namespace MoogModule.Daemon
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => OnExit();

            Console.WriteLine("Hi, World!");
            Console.ReadKey();

            long index = 0;

            var intervalExecutor = new IntervalExecutor(TimeSpan.FromMilliseconds(1));
            intervalExecutor.OnTick += () =>
                {
                    if ((index % (10 * 1_000) == 0) && (index != 0)) // every 10 sec
                        Console.WriteLine($"AverageInterval = {intervalExecutor.AverageInterval.TotalMilliseconds}ms");

                    if (++index > (1 * 60 * 1_000)) // after 1 min
                    {
                        intervalExecutor.Stop();
                        Console.WriteLine("By, World!");
                        Console.ReadKey();
                    };
                };

            SystemOptimizer.Optimize();
            intervalExecutor.Start();
        }

        static void OnExit()
        {
            SystemOptimizer.ResetToDefault();
        }
    }
}