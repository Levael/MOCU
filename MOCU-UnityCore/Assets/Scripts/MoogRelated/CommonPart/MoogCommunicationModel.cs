using System;
using System.Linq;
using System.Collections.Generic;

using DaemonsRelated;


namespace MoogModule
{
    public class MoogDataTransferObject : IDataTransferObject
    {
        // Commands
        public bool ConnectCommand                                      { get; set; } = false;
        public bool EngageCommand                                       { get; set; } = false;
        public bool DisengageCommand                                    { get; set; } = false;
        public bool ResetCommand                                        { get; set; } = false;
        public bool DoReceiveFeedback                                   { get; set; } = false;
        public bool MoveToPointCommand                                  { get; set; } = false;
        public bool MoveByTrajectoryCommand                             { get; set; } = false;

        // Parameters
        public ConnectParameters? ConnectParameters                     { get; set; } = null;
        public MoveToPointParameters? MoveToPointParameters             { get; set; } = null;
        public MoveByTrajectoryParameters? MoveByTrajectoryParameters   { get; set; } = null;

        // Info
        public MachineState MoogState                                   { get; set; } = MachineState.Disabled;
        public string Faults                                            { get; set; } = String.Empty;

        // Common for every DTO
        public IEnumerable<DaemonErrorReport> DaemonErrorReports        { get; set; } = Enumerable.Empty<DaemonErrorReport>();
        public bool DoTerminateTheDaemon                                { get; set; } = false;
        public string CustomMessage                                     { get; set; } = String.Empty;
    }



    public class ConnectParameters
    {
        public DofParameters StartPosition                              { get; set; } = default;
        public double MaxAcceleration                                   { get; set; } = 0.0;
        public string HOST_IP                                           { get; set; } = String.Empty;
        public string HOST_PORT                                         { get; set; } = String.Empty;
        public string MBC_IP                                            { get; set; } = String.Empty;
        public string MBC_PORT                                          { get; set; } = String.Empty;
    }

    public class MoveToPointParameters
    {
        public DofParameters Coordinate                                 { get; set; } = default;
        public DateTime ScheduledTime                                   { get; set; } = DateTime.MinValue;
    }

    public class MoveByTrajectoryParameters
    {
        public IEnumerable<DofParameters> Coordinates                   { get; set; } = Enumerable.Empty<DofParameters>();
        public TimeSpan MovementDuration                                { get; set; } = TimeSpan.Zero;
        public DateTime ScheduledTime                                   { get; set; } = DateTime.MinValue;
    }
}