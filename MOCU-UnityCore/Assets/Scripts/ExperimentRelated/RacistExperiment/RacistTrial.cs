using System;


namespace RacistExperiment
{
    public class RacistTrial
    {
        public TwoIntervalExperimentHalf FirstInterval;
        public TwoIntervalExperimentHalf SecondInterval;
        public RacistAnswer CorrectAnswer = RacistAnswer.None;
        public RacistAnswer ReceivedAnswer = RacistAnswer.None;
        public DateTime StartedAt;
        public DateTime FinishedAt;
    }
}