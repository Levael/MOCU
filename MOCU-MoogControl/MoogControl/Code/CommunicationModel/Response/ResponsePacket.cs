using System.Runtime.InteropServices;


namespace MoogModule.Daemon
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ResponsePacket
    {
        public FaultData FaultFlags;
        public IoInfo IoInfoFlags;
        public MachineState MachineState;
        public DofParameters Parameters;
        public float Spare;
    }
}