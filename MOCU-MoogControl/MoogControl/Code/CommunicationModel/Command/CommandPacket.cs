using System.Runtime.InteropServices;


namespace MoogModule.Daemon
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CommandPacket
    {
        public MotionCommandWord MCW;
        public DofParameters Parameters;
        public float Spare;
    }
}