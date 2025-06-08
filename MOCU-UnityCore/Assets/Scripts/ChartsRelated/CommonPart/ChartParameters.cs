using System.Linq;
using System.Collections.Generic;


namespace ChartsModule
{
    public class ChartParameters
    {
        public IEnumerable<ChartPointData> Points   { get; set; } = Enumerable.Empty<ChartPointData>();
        public string Title                         { get; set; } = "Interactive Plot";
        public string XLabel                        { get; set; } = "X";
        public string YLabel                        { get; set; } = "Y";
        public string LineColor                     { get; set; } = "#0000FF";
    }
}