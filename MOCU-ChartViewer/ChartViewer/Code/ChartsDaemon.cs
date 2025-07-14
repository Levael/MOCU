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

            _hostAPI.GenerateChartAsImage += OnGenerateChartAsImage;
            _hostAPI.GenerateChartAsForm += OnGenerateChartAsForm;
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

        // ...............................

        private void OnGenerateChartAsImage(ChartData chartData)
        {
            // todo
            //ChartMaker.SavePng(chartData);
        }

        private void OnGenerateChartAsForm(ChartData chartData)
        {
            ChartMaker.ShowInteractive(chartData);
        }
    }
}