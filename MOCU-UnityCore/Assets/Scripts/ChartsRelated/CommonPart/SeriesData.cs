
using System;
using System.Linq;
using System.Collections.Generic;


namespace ChartsModule
{
    public class SeriesData
    {
        public IEnumerable<PointData> Series    { get; set; } = Enumerable.Empty<PointData>();
        public string Title                     { get; set; } = String.Empty;
        public bool ConnectPoints               { get; set; } = true;

        // todo: maybe add line settings (style, size, color, etc)
    }
}