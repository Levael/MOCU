using System;

using DaemonsRelated.DaemonPart;


namespace ChartsModule.Daemon
{
    internal class ChartsDaemon : IDaemonLogic
    {
        public event Action<string> TerminateDaemon;

        private readonly ChartsDaemonSideBridge _hostAPI;

        public ChartsDaemon(ChartsDaemonSideBridge hostAPI)
        {
            _hostAPI = hostAPI;
            _hostAPI.TerminateDaemon += message => Console.WriteLine("Got command to terminate the daemon.");
        }

        public void DoBeforeExit()
        {
            //
        }

        public void Run()
        {
            _hostAPI.StartCommunication();
            Console.WriteLine("Started");
        }
    }
}