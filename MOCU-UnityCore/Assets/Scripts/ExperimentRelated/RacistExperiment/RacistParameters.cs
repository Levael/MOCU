using System;
using System.Collections.Generic;

using MoogModule;


namespace RacistExperiment
{
    public class RacistParameters
    {
        public IReadOnlyList<float> Multipliers         { get; set; } = new List<float> { 1.125f, 1.25f, 1.5f, 1.75f, 2.0f }.AsReadOnly();

        public int TrialsNumber                         { get; set; } = 0;      // Better be modulus 4
        public float ReferenceDistance                  { get; set; } = 0.08f;  // meters
        public DofParameters StartPosition              { get; set; } = new DofParameters { Heave = -0.22f, Surge = -0.08f };
        public RacistStimulusType StimulusType          { get; set; } = RacistStimulusType.None;
        public float StartDistanceToTarget              { get; set; } = 0.66f;

        public TimeSpan FirstMovementDuration           { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan SecondMovementDuration          { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan BackwardMovementDuration        { get; set; } = TimeSpan.FromSeconds(2.5);
        public TimeSpan PauseBetweenIntervalsDuration   { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan DelayBeforeStartMoving          { get; set; } = TimeSpan.FromSeconds(0.5);
        public TimeSpan TimeToAnswer                    { get; set; } = TimeSpan.FromSeconds(2);

        public bool PlayStartTrialSound                 { get; set; } = true;
        public bool PlayFinishTrialSound                { get; set; } = true;
        public bool PlayTimeoutSound                    { get; set; } = true;
        public bool PlayAnswerReceivedSound             { get; set; } = true;
        public bool PlayCorrectAnswerSound              { get; set; } = false;
        public bool PlayIncorrectAnswerSound            { get; set; } = false;

        public float ChanceToMakeTaskHarder             { get; set; } = 0.8f;
        public float ChanceToMakeTaskEasier             { get; set; } = 0.8f;
    }
}