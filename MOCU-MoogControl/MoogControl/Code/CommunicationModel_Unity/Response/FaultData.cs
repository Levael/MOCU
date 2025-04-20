using System;


namespace MoogModule.Daemon
{
    [Flags]
    public enum LatchedFaultWord1 : uint
    {
        None                        = 0,
        HostCommunicationTimeout    = 1u << 0,      // Alarm ID: 401
        HomeConflictFault           = 1u << 9,      // Alarm ID: 612
        DriveSettlingActive         = 1u << 31      // Alarm ID: 501
    }

    [Flags]
    public enum LatchedFaultWord2 : uint
    {
        None                        = 0,
        LostACPower                 = 1u << 0,      // Alarm ID: 146
        FollowingError              = 1u << 4,      // Alarm ID: 809
        HitHomeWhileMoving          = 1u << 5,      // Alarm ID: 154-159
        HostCommandRangeExceeded    = 1u << 6,      // Alarm ID: 400
        EmergencyShutOffActive      = 1u << 15,     // Alarm ID: 164
        ESTOP                       = 1u << 16,     // Alarm ID: 147
        SafetyRelayOpen             = 1u << 20,     // Alarm ID: 160
        SafetyRelayResponse         = 1u << 21,     // Alarm ID: 161
        Interlock                   = 1u << 22,     // Alarm ID: 122
        DriveInitializationFailure  = 1u << 23,     // Alarm ID: 700
        DriveCommunication          = 1u << 24,     // Alarm ID: 702
        PLCSystemFault              = 1u << 25      // Alarm ID: 150
    }

    [Flags]
    public enum LatchedFaultWord3 : uint
    {
        None                        = 0,
        AccelerationFault           = 1u << 0,      // Alarm ID: 800
        VelocityFault               = 1u << 1,      // Alarm ID: 801
        EnvelopeExtendLimit         = 1u << 3,      // Alarm ID: 813
        EnvelopeRetractLimit        = 1u << 4       // Alarm ID: 812
    }
}