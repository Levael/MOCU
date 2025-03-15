using System.Collections.Generic;
using System;
using System.Linq;

namespace MoogModule.Daemon
{
    public static class ResponsePacketParser
    {
        //private static readonly HashSet<MachineState> ValidStates = Enum.GetValues<MachineState>().ToHashSet();

        //public static bool IsValid(ResponsePacket packet) => ValidStates.Contains(packet.MachineState);
        //public static bool HasFault(ResponsePacket packet) => packet.FaultFlags != FaultData.None;
        //public static List<FaultData> GetFaults(ResponsePacket packet) => Enum.GetValues<FaultData>().Where(f => f != FaultData.None && packet.FaultFlags.HasFlag(f)).ToList();
    }
}