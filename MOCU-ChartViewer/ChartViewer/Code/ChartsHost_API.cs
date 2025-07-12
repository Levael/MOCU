using System;


namespace ChartsModule.Daemon
{
    public interface ChartsHost_API  // Is a mirror of 'ChartsDaemon_API'
    {
        void ChartImageGenerated(string path);

        event Action<ChartData> GenerateChartAsImage;
        event Action<ChartData> GenerateChartAsForm;
    }
}