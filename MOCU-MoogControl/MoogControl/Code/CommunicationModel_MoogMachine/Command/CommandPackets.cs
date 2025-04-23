/*
 * COMMANDS (SCC to MBC) (for DOF mode only)
 * (part of the documentation, taken from the file 'CDS7330 INTERFACE DEFINITION MANUAL.pdf')
 * 
 * 
 */


namespace MoogModule.Daemon
{
    public static class CommandPackets
    {
        public static CommandPacket Engage(DofParameters dof)       => new (motionCommandWord: MotionCommandWord.Engage, dofParameters: dof);
        public static CommandPacket Disengage()                     => new (motionCommandWord: MotionCommandWord.Disengage);
        public static CommandPacket Reset()                         => new (motionCommandWord: MotionCommandWord.Reset);
        public static CommandPacket NewPosition(DofParameters dof)  => new (motionCommandWord: MotionCommandWord.Null, dofParameters: dof);
    }
}




// WRONG !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

/*
 * COMMANDS (SCC to MBC) (for DOF mode only)
 * (part of the documentation, taken from the file '6DOF2000E Real Time Ethernet Interface Definition - CDS7237.pdf')
 * 
 * 
 * 
 * DISABLE  (Valid in ALL States)
 * 1) The MB disables Ethernet communications, ignoring further commands;
 * 2) Removes power from the motor controllers;
 * 3) Returns to HOME position under battery power.
 * Reset is manual, the user must remove & re-apply power to MB or re-boot the Motion Base Computer.
 * 
 * PARK (Valid only in ENGAGED or STANDBY States)
 * MB returns to PARK position under power, then removes power from the motors.
 * 
 * ENGAGE (Valid only in IDLE State)
 * Makes MB ready to run (applies power to the motor controllers).
 * To engage, communications must be OK, machine state must be idle, and the base must be "at home".
 * The following actuator parameters must be sent from the SCC: all DOFs must be = 0.0 m or 0.0 rad.
 * The MBC software powers and enables the amplifiers, then moves the base up to the starting position listed above.
 * At this point the machine state becomes ENGAGED, and the SCC can control the actuator movement.
 * The SCC must keep the command data at these values until the base is ENGAGED to avoid abrupt motions.
 * 
 * START (Valid only in IDLE State)
 * Same as 'ENGAGE', except the customer may define the starting heave position of the base.
 * Valid start positions ranges: all commands must be at neutral = 0.0m or 0.0 rad, except for heave, which may range from 0.0m to –0.4572m.
 * Words 1 through 6 indicate the start position.
 * These words must not change until after the base has reached the ENGAGED state to avoid abrupt motions when the base becomes ENGAGED.
 * 
 * DOF MODE (Valid only in IDLE and POWER UP States)
 * Advises the MB to interpret the actuator command data as degrees of freedom.
 * Data is ordered as roll, pitch, heave, surge, yaw, lateral, in the command frame.
 * For the DOF mode, DOF commands are all 32 bit float values.
 * Values for the platform angles / position are defined in radians (rad) or meter (m).
 * DOF mode is the default mode of MB operation.
 * 
 * RESET (Valid only in FAULT and INHIBIT States)
 * Used to recover a MB from FAULT_2 state. Also restores normal operation after the INHIBIT command is received.
 * 
 * INHIBIT (Valid only in IDLE and POWER UP States)
 * A temporary means of de-activating the MB. The MB ignores further commands until the RESET command is received.
 * 
 * NEW POSITION (May be sent in ALL States)
 * Provides position command data to the MB.
 * The MB in the ENGAGED state will update its position loops with the data provided in the command frame.
 */