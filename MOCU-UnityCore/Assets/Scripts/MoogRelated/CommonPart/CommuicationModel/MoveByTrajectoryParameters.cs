#nullable enable

using System;


namespace MoogModule
{
    public class MoveByTrajectoryParameters
    {
        public TimeSpan MovementDuration                { get; set; } = TimeSpan.Zero;
        public DateTime ScheduledStartTime              { get; set; } = DateTime.MinValue;
        public DofParameters StartPoint                 { get; set; } = default;
        public DofParameters EndPoint                   { get; set; } = default;
        public TrajectoryType TrajectoryType            { get; set; } = TrajectoryType.None;
        public DelayCompensationStrategy DelayHandling  { get; set; } = DelayCompensationStrategy.None;
        public object? TrajectorySpetialSettings        { get; set; } = null;
    }
}