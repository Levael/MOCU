
using System;
using System.Linq;
using System.Collections.Generic;


namespace ChartsModule
{
    public class SeriesData
    {
        public IEnumerable<PointData> Series    { get; set; } = Enumerable.Empty<PointData>();
        public string Title                     { get; set; } = String.Empty;
        public string PointsColor               { get; set; } = "#0000FF";
        public string LineColor                 { get; set; } = "#0000FF";
        public bool ConnectPoints               { get; set; } = true;
    }
}