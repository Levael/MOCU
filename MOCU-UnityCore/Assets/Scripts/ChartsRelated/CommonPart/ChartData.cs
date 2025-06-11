using System;
using System.Linq;
using System.Collections.Generic;


namespace ChartsModule
{
    public class ChartData
    {
        public IEnumerable<SeriesData> Series   { get; set; } = Enumerable.Empty<SeriesData>();
        public string Title                     { get; set; } = String.Empty;
        public string XLabel                    { get; set; } = "X";
        public string YLabel                    { get; set; } = "Y";
    }
}