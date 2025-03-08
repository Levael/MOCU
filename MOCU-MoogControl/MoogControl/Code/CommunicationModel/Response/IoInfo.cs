using System;


namespace MoogModule.Daemon
{
    [Flags]
    public enum IoInfo : uint
    {
        None                    = 0b0000_0000,

        // Low byte (bits d7-d0)
        EStopSense              = 0b1000_0000, // d7
        AmplifierEnableCommand  = 0b0100_0000, // d6
        DriveBusSense           = 0b0010_0000, // d5
        LimitShuntCommand       = 0b0001_0000, // d4
        LimitSwitchSense        = 0b0000_1000, // d3
        AmplifierFaultSense     = 0b0000_0100, // d2
        ThermalFaultSense       = 0b0000_0010, // d1
        BaseAtHome              = 0b0000_0001, // d0
    }
}