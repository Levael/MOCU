namespace ChartsModule.Daemon
{
    public class InteractiveChart
    {
        public void Run()
        {
            /*
            if (args.Length != 1 || string.IsNullOrWhiteSpace(args[0]))
            {
                MessageBox.Show("Expected a single path to JSON as argument.", "Invalid Arguments", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string jsonPath = args[0];

            if (!File.Exists(jsonPath))
            {
                MessageBox.Show($"File not found:\n{jsonPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ChartParameters parameters;

            try
            {
                string json = File.ReadAllText(jsonPath);
                parameters = JsonHelper.DeserializeJson<ChartParameters>(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read or parse JSON:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var form = new Form
            {
                Text = parameters.Title,
                Width = 800,
                Height = 600
            };

            var formsPlot = new ScottPlot.WinForms.FormsPlot
            {
                Dock = DockStyle.Fill
            };

            var plot = formsPlot.Plot;

            foreach (ChartPointData point in parameters.Points)
            {
                var marker = plot.Add.Marker(point.X, point.Y);
                marker.Size = point.PointSize;
                marker.MarkerColor = new ScottPlot.Color(point.PointColor);
            }

            plot.Title(parameters.Title);
            plot.XLabel(parameters.XLabel);
            plot.YLabel(parameters.YLabel);

            form.Controls.Add(formsPlot);
            Application.Run(form);
            */
        }
    }
}