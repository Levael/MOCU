using System;

using RacistExperiment;


namespace MeshisExperiment
{
    public class MeshisTrial
    {
        public TwoIntervalMeshisExperimentHalf FirstInterval;
        public TwoIntervalMeshisExperimentHalf SecondInterval;
        public MeshisAnswer CorrectAnswer = MeshisAnswer.None;
        public MeshisAnswer ReceivedAnswer = MeshisAnswer.None;
        public DateTime StartedAt;
        public DateTime FinishedAt;
    }
}