using System;
using System.Collections.Generic;

using MoogModule;
using RacistExperiment;


namespace MeshisExperiment
{
    public class MeshisParameters
    {
        public IReadOnlyList<float> Multipliers         { get; set; } = new List<float> { 0.5f, 0.75f, 1f, 1.125f, 1.25f, 1.5f, 1.75f, 2.0f }.AsReadOnly();

        public int TrialsNumber                         { get; set; } = 0;      // Better be modulus 4
        public float ReferenceDistance                  { get; set; } = 0.08f;  // meters
        public DofParameters MoogStartPosition          { get; set; } = new DofParameters { Heave = -0.22f, Surge = -0.08f };
        public DofParameters CameraStartPosition        { get; set; } = new DofParameters { Heave = 1.7f };
        public RacistExperimentStimulusType StimulusType          { get; set; } = RacistExperimentStimulusType.None;
        public float StartDistanceToStarCenter          { get; set; } = 0.66f;
        public float StarSize                           { get; set; } = 0.05f;
        public float StarsQuantity                      { get; set; } = 100f;
        public float StarsBoxSideLength                 { get; set; } = 1f;

        public TimeSpan FirstMovementDuration           { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan SecondMovementDuration          { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan BackwardMovementDuration        { get; set; } = TimeSpan.FromSeconds(2.5);
        public TimeSpan PauseBetweenIntervalsDuration   { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan DelayBeforeStartSound           { get; set; } = TimeSpan.FromSeconds(1.5);
        public TimeSpan DelayAfterStartSignal           { get; set; } = TimeSpan.FromSeconds(0.5);
        public TimeSpan TimeToAnswer                    { get; set; } = TimeSpan.FromSeconds(2);
        public TimeSpan DelayAfterAnswerSignal          { get; set; } = TimeSpan.FromSeconds(0);

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