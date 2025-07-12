using ScottPlot;
using ScottPlot.Statistics;
using ScottPlot.WinForms;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace ChartsModule
{
    public static class ChartMaker
    {
        public static void SavePng(ChartData data, string filename)
        {
            var plot = new ScottPlot.Plot();
            PlotData(plot, data);
            plot.SavePng(filename, data.Width, data.Height);
        }

        public static void ShowInteractive(ChartData data)
        {
            Thread thread = new Thread(() =>
            {
                var form = new Form
                {
                    Width = data.Width,
                    Height = data.Height,
                    Text = data.Title
                };

                var formsPlot = new FormsPlot { Dock = DockStyle.Fill };
                form.Controls.Add(formsPlot);

                PlotData(formsPlot.Plot, data);

                Application.Run(form);
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        // ........................................................................................

        private static void PlotData(ScottPlot.Plot plot, ChartData data)
        {
            var legendEntries = new HashSet<string>();

            foreach (var series in data.Series)
            {
                double[] xs = series.Series.Select(p => p.X).ToArray();
                double[] ys = series.Series.Select(p => p.Y).ToArray();

                ScottPlot.Color seriesColor = ScottPlot.Color.FromHex(series.Color);
                float pointSize = series.PointSize;
                float lineSize = series.LineSize;

                string legendKey = $"{series.Title}_{series.Color}";

                if (series.ConnectPoints)
                {
                    var scatter = plot.Add.Scatter(xs, ys);
                    scatter.LineWidth = lineSize;
                    scatter.MarkerSize = pointSize;
                    scatter.Color = seriesColor;

                    if (!legendEntries.Contains(legendKey))
                    {
                        scatter.LegendText = series.Title;
                        legendEntries.Add(legendKey);
                    }
                }
                else
                {
                    var markers = plot.Add.Markers(xs, ys);
                    markers.MarkerSize = pointSize;
                    markers.Color = seriesColor;

                    if (!legendEntries.Contains(legendKey))
                    {
                        markers.LegendText = series.Title;
                        legendEntries.Add(legendKey);
                    }
                }

                foreach (var point in series.Series)
                {
                    if (!string.IsNullOrEmpty(point.Label))
                        plot.Add.Text(point.Label, point.X, point.Y);
                }
            }

            plot.Axes.Bottom.Label.Text = data.XLabel;
            plot.Axes.Left.Label.Text = data.YLabel;
            plot.FigureBackground = new BackgroundStyle { Color = ScottPlot.Color.FromHex(data.BackgroundColor) };
            plot.Legend.IsVisible = data.DoShowLegend;
        }
    }
}