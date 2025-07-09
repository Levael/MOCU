using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ScottPlot;
using ScottPlot.WinForms;
using SDColor = System.Drawing.Color;


namespace ChartsModule.Daemon
{
    public static class ChartMaker
    {
        public static void InteractiveChart(ChartData data, ChartConversionSettings? settings = null, int width = 1000, int height = 600)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            settings ??= new ChartConversionSettings();

            var thread = new Thread(() =>
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                using var form = new Form { Text = string.IsNullOrWhiteSpace(data.Title) ? "Chart" : data.Title, ClientSize = new System.Drawing.Size(width, height) };
                var fp = new FormsPlot { Dock = DockStyle.Fill };
                Render(fp.Plot, data, settings);
                form.Controls.Add(fp);
                form.FormClosed += (_, __) => Application.ExitThread();
                Application.Run(form);
            })
            { IsBackground = true };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public static string ChartImage(ChartData data, string? directory = null, string? fileName = null, int width = 1920, int height = 1080, ChartConversionSettings? settings = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            settings ??= new ChartConversionSettings();

            var plt = new Plot();
            Render(plt, data, settings);

            directory ??= Path.GetTempPath();
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            fileName ??= $"chart_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var fullPath = Path.Combine(directory, fileName);
            plt.SavePng(fullPath, width, height);
            return Path.GetFullPath(fullPath);
        }

        // ........................................................................................

        private static SDColor ParseColor(string? hex, SDColor fallback)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return fallback;
            try { return System.Drawing.ColorTranslator.FromHtml(hex); }
            catch { return fallback; }
        }

        private static ScottPlot.Color ToSpColor(SDColor c) => new(c.R, c.G, c.B, c.A);

        private static void Render(Plot plt, ChartData chartData, ChartConversionSettings _)
        {
            plt.Clear();
            if (chartData?.Series == null || !chartData.Series.Any())
                return;

            foreach (var series in chartData.Series)
            {
                var points = series.Series?.ToArray() ?? Array.Empty<PointData>();
                if (points.Length == 0) continue;

                SDColor defaultPointColor = ParseColor(series.PointsColor, SDColor.DodgerBlue);
                double defaultPointSize = points.First().PointSize > 0 ? points.First().PointSize : 5;
                SDColor lineClrSys = ParseColor(series.LineColor, defaultPointColor);
                var lineColor = ToSpColor(lineClrSys);

                if (series.ConnectPoints)
                {
                    double[] xsLine = points.Select(p => p.X).ToArray();
                    double[] ysLine = points.Select(p => p.Y).ToArray();
                    var lineScatter = plt.Add.Scatter(xsLine, ysLine);
                    if (!string.IsNullOrWhiteSpace(series.Title))
                        lineScatter.LegendText = series.Title;
                    lineScatter.LineWidth = 1;
                    lineScatter.Color = lineColor;
                    lineScatter.MarkerSize = 0;
                }

                var groups = points.GroupBy(p => new
                {
                    Color = ParseColor(p.PointColor, defaultPointColor),
                    Size = p.PointSize > 0 ? p.PointSize : defaultPointSize
                });

                bool firstGroup = true;
                foreach (var g in groups)
                {
                    double[] xs = g.Select(p => p.X).ToArray();
                    double[] ys = g.Select(p => p.Y).ToArray();
                    var scatter = plt.Add.Scatter(xs, ys);
                    scatter.Color = ToSpColor(g.Key.Color);
                    scatter.MarkerSize = (float)g.Key.Size;
                    scatter.LineWidth = 0;
                    if (firstGroup && !string.IsNullOrWhiteSpace(series.Title))
                    {
                        scatter.LegendText = series.Title;
                        firstGroup = false;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(chartData.Title))
                plt.Title(chartData.Title);
            plt.XLabel(chartData.XLabel);
            plt.YLabel(chartData.YLabel);

            if (chartData.Series.Any(s => !string.IsNullOrWhiteSpace(s.Title)))
            {
                plt.Legend.IsVisible = true;
                plt.Legend.Alignment = Alignment.UpperRight;
            }
        }
    }
}