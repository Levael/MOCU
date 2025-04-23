using System;
using System.Numerics;


namespace MoogModule.Daemon
{
    public class MoogRealTimeState
    {
        public DateTime TimeOfLastCommand { get; set; }
        public DateTime TimeOfLastFeedback { get; set; }
        public DofParameters LastCommandPosition { get; set; }
        public DofParameters LastFeedbackPosition { get; set; }
        public Vector3 Velocity { get; set; } = Vector3.Zero;
        public Vector3 Acceleration { get; set; } = Vector3.Zero;
        public EncodedMachineState EncodedMachineState { get; set; } = EncodedMachineState.Disabled;
        public string Faults { get; set; } = String.Empty;
    }
}