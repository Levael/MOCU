namespace MoogModule.Daemon
{
    public enum MachineState : uint
    {
        // d4-d7 (1010) = feedback type and mode are both DOF

        PowerUp     = 0b1010_0000,
        Idle        = 0b1010_0001,
        Standby     = 0b1010_0010,
        Engaged     = 0b1010_0011,
        Parking     = 0b1010_0111,
        Fault1      = 0b1010_1000,
        Fault2      = 0b1010_1001,
        Fault3      = 0b1010_1010,
        Disabled    = 0b1010_1011,
        Inhibited   = 0b1010_1100
    }
}