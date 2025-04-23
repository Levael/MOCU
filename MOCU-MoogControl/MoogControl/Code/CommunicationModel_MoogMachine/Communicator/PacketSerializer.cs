using System;
using System.Runtime.InteropServices;


namespace MoogModule.Daemon
{
    public static class PacketSerializer
    {
        private static readonly int CommandSize     = Marshal.SizeOf<CommandPacket>();
        private static readonly int ResponseSize    = Marshal.SizeOf<ResponsePacket>();

        public static byte[] Serialize(CommandPacket value)
        {
            byte[] buffer = new byte[CommandSize];
            MemoryMarshal.Write(buffer, in value);
            ConvertEndian(buffer);
            return buffer;
        }

        public static ResponsePacket Deserialize(byte[] data)
        {
            if (data.Length != ResponseSize)
                throw new ArgumentException($"Invalid packet size. Expected {ResponseSize} bytes, got {data.Length} bytes.");

            ConvertEndian(data);
            return MemoryMarshal.Read<ResponsePacket>(data);
        }

        private static void ConvertEndian(byte[] byteArray)
        {
            for (int offset = 0; offset < byteArray.Length; offset += 4)
                Array.Reverse(byteArray, offset, 4);
        }
    }
}