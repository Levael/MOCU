using System;
using System.Collections.Generic;

using Temporal;
using MoogModule;


namespace RacistExperiment
{
    public class RacistParameters
    {
        public IReadOnlyList<float> Multipliers             { get; set; } = new List<float> { 0.5f, 0.75f, 1.125f, 1.25f, 1.5f, 1.75f, 2.0f }.AsReadOnly();

        public int TrialsNumber                             { get; set; } = 0;      // Better be modulus 4
        public float ReferenceDistance                      { get; set; } = 0.08f;  // meters
        public DofParameters CameraStartPosition            { get; set; } = new DofParameters { Surge = 0, Heave = 1.7f, Sway = 0 };
        public DofParameters MoogStartPosition              { get; set; } = new DofParameters { Surge = -0.12f, Heave = -0.22f, Sway = 0 };
        public TemporalExperimentStimulusType StimulusType  { get; set; } = TemporalExperimentStimulusType.None;
        public float StartDistanceToTarget                  { get; set; } = 0.66f;
        public TimeSpan DelayBetweenMoogAndVr               { get; set; } = TimeSpan.FromMilliseconds(100); // Moog starts with delay
        public float MoogMovementDurationCorrectionFactor   { get; set; } = 0.8f;   // It takes to Moog more time (1 / 1.25) ~1100ms (-100 from prev param)

        public TimeSpan FirstMovementDuration               { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan SecondMovementDuration              { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan BackwardMovementDuration            { get; set; } = TimeSpan.FromSeconds(3);
        public TimeSpan PauseBetweenIntervalsDuration       { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan DelayBeforeStartSound               { get; set; } = TimeSpan.FromSeconds(1.5);
        public TimeSpan DelayAfterStartSignal               { get; set; } = TimeSpan.FromSeconds(0.5);
        public TimeSpan TimeToAnswer                        { get; set; } = TimeSpan.FromSeconds(2);
        public TimeSpan DelayAfterAnswerSignal              { get; set; } = TimeSpan.FromSeconds(0);

        public bool PlayStartTrialSound                     { get; set; } = true;
        public bool PlayFinishTrialSound                    { get; set; } = true;
        public bool PlayTimeoutSound                        { get; set; } = true;
        public bool PlayAnswerReceivedSound                 { get; set; } = true;
        public bool PlayCorrectAnswerSound                  { get; set; } = false;
        public bool PlayIncorrectAnswerSound                { get; set; } = false;

        public float ChanceToMakeTaskHarder                 { get; set; } = 0.8f;
        public float ChanceToMakeTaskEasier                 { get; set; } = 0.8f;
    }
}