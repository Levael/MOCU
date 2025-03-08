using System;


namespace MoogModule.Daemon
{
    [Flags]
    public enum FaultData : uint
    {
        None                = 0b0000_0000_0000_0000,

        // High byte (bits d15-d8)
        EStop               = 0b1000_0000_0000_0000, // d15
        SnubberFault        = 0b0100_0000_0000_0000, // d14
        ActuatorRunaway     = 0b0010_0000_0000_0000, // d13
        BatteryFault        = 0b0001_0000_0000_0000, // d12
        LowIdleRate         = 0b0000_1000_0000_0000, // d11
        MotorThermalFault   = 0b0000_0100_0000_0000, // d10
        CommandRangeError   = 0b0000_0010_0000_0000, // d9
        InvalidFrame        = 0b0000_0001_0000_0000, // d8

        // Low byte (bits d7-d0)
        WatchdogFault       = 0b0000_0000_1000_0000, // d7
        LimitSwitchFault    = 0b0000_0000_0100_0000, // d6
        DriveBusFault       = 0b0000_0000_0010_0000, // d5
        AmplifierFault      = 0b0000_0000_0001_0000, // d4
        CommFault           = 0b0000_0000_0000_1000, // d3
        HomingFault         = 0b0000_0000_0000_0100, // d2
        EnvelopeFault       = 0b0000_0000_0000_0010, // d1
        TorqueMonFault      = 0b0000_0000_0000_0001, // d0
    }
}