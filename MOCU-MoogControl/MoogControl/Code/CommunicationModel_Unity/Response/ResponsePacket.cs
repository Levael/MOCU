using System.Runtime.InteropServices;

using MoogModule;


namespace MoogModule.Daemon
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ResponsePacket
    {
        // Header
        public readonly uint PacketLength;
        public readonly uint PacketSequenceCount;
        public readonly uint ReservedForFutureUse;
        public readonly uint MessageId; // should be 200

        // Payload
        public readonly MachineState MachineStatusWord;
        public readonly IoInfo DiscreteIOWord;
        public readonly LatchedFaultWord1 FaultPart1;
        public readonly LatchedFaultWord2 FaultPart2;
        public readonly LatchedFaultWord3 FaultPart3;
        public readonly uint OptionalStatusData;        // should be 1
        public readonly DofParameters Parameters;
        public readonly uint OptionalStatusDataAgain;   // should be 1
    }
}