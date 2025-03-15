using System.Runtime.InteropServices;


namespace MoogModule.Daemon
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct CommandPacket
    {
        // Header
        public readonly uint PacketLength;
        public readonly uint PacketSequenceCount;
        public readonly uint ReservedForFutureUse;
        public readonly uint MessageId;

        // Payload
        public readonly MotionCommandWord MCW;
        public readonly uint StatusResponseWord;
        public readonly DofParameters Parameters;
        public readonly uint SpecialEffectsActiveCommand;



        // Constructor (not a part of final packet)
        public CommandPacket(MotionCommandWord motionCommandWord, DofParameters dofParameters = default)
        {
            PacketLength = 36;                          // 9*4=36 (9 lines, every one is uint = 4 bytes)
            PacketSequenceCount = ++_sequenceCounter;   // unique packet id
            ReservedForFutureUse = 0;                   // Spare field
            MessageId = 100;                            // 100 = DOF Command Mode Data

            MCW = motionCommandWord;
            StatusResponseWord = 0x07D00040;            // DOF mode, 2000 Hz
            Parameters = dofParameters;
            SpecialEffectsActiveCommand = 0;            // idk what it is
        }

        // Static increment for auto-assign id (not a part of final packet).
        // Note that the ID is created when calling new(), not when copying.
        private static volatile uint _sequenceCounter = 0;
    }
}