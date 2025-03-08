using System.Runtime.InteropServices;


namespace MoogModule.Daemon
{
    public static class PacketSerializer<T> where T : struct
    {
        // todo: store premade array (for both command and response packets)

        public static byte[] Serialize(T value)
        {
            byte[] buffer = new byte[Marshal.SizeOf<T>()];
            MemoryMarshal.Write(buffer, in value);
            return buffer;
        }

        public static T Deserialize(byte[] data)
        {
            return MemoryMarshal.Read<T>(data);
        }
    }
}