using System;
using System.Collections.Generic;
using System.Linq;


namespace MoogModule
{
    public class MoogFeedback
    {
        public IEnumerable<(DateTime timestamp, DofParameters coordinate)> Commands     { get; set; } = Enumerable.Empty<(DateTime, DofParameters)>();
        public IEnumerable<(DateTime timestamp, DofParameters coordinate)> Responses    { get; set; } = Enumerable.Empty<(DateTime, DofParameters)>();
        public IEnumerable<(DateTime timestamp, string error)> Errors                   { get; set; } = Enumerable.Empty<(DateTime, string)>();
        public IEnumerable<(DateTime timestamp, EncodedMachineState state)> States      { get; set; } = Enumerable.Empty<(DateTime, EncodedMachineState)>();
    }
}