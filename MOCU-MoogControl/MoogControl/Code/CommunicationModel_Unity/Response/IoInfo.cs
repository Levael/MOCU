using System;


namespace MoogModule.Daemon
{
    [Flags]
    public enum IoInfo : uint
    {
        None                = 0,
        MaintenanceMode     = 1u << 0,  // 1-MBC is in Maintenance Mode, 0-MBC is in Normal (Host) Mode
        BaseAtHome          = 1u << 1,  // 1-All actuators are at-home, 0-One or more actuators are not at home
        EStopInputActive    = 1u << 2   // 1-ESTOP is asserted, 0-ESTOP not asserted
    }
}