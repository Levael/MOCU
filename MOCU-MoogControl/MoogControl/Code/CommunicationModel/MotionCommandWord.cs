namespace MoogModule.Daemon
{
    public enum MotionCommandWord : uint
    {
        Disable         = 0xDC,
        Park            = 0xD2,
        Engage          = 0xB4,
        Start           = 0xAF,
        DofMode         = 0xAA,
        Reset           = 0xA0,
        Inhibit         = 0x96,
        NewPosition     = 0x82
    }
}