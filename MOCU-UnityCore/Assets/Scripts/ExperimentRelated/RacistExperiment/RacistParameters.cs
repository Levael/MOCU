using System.Collections.Generic;

namespace RacistExperiment
{
    public class RacistParameters
    {
        public readonly IReadOnlyList<float> Multipliers = new List<float> { 1.125f, 1.25f, 1.5f, 1.75f, 2.0f }.AsReadOnly();
    }
}