using System.Collections.Generic;
using System;
using System.Linq;


namespace MoogModule.Daemon
{
    public static class ResponsePacketParser
    {
        public static void Test(ResponsePacket parsedMessage)
        {
            Console.WriteLine($"\nReceived message:\n" +
                    $"PacketLength - {parsedMessage.PacketLength}. Should be - 52\n" +
                    $"PacketSequenceCount - {parsedMessage.PacketSequenceCount}\n" +
                    $"ReservedForFutureUse - {parsedMessage.ReservedForFutureUse}. Should be - 0\n" +
                    $"MessageId - {parsedMessage.MessageId}. Should be - 200\n" +

                    $"MachineStatusWord - {parsedMessage.MachineStatusWord}\n" +
                    $"DiscreteIOWord - {parsedMessage.DiscreteIOWord}\n" +
                    $"FaultPart1 - {parsedMessage.FaultPart1}\n" +
                    $"FaultPart2 - {parsedMessage.FaultPart2}\n" +
                    $"FaultPart3 - {parsedMessage.FaultPart3}\n" +
                    $"OptionalStatusData - {parsedMessage.OptionalStatusData}. Should be - 1\n" +
                    $"Parameters (heave) - {parsedMessage.Parameters.Heave}\n" +
                    $"OptionalStatusDataAgain - {parsedMessage.OptionalStatusDataAgain}. Should be - 0\n");
        }
        //private static readonly HashSet<MachineState> ValidStates = Enum.GetValues<MachineState>().ToHashSet();

        //public static bool IsValid(ResponsePacket packet) => ValidStates.Contains(packet.MachineState);
        //public static bool HasFault(ResponsePacket packet) => packet.FaultFlags != FaultData.None;
        //public static List<FaultData> GetFaults(ResponsePacket packet) => Enum.GetValues<FaultData>().Where(f => f != FaultData.None && packet.FaultFlags.HasFlag(f)).ToList();
    }
}