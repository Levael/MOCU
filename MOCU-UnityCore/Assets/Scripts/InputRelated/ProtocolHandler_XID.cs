/*using System;
using System.IO.Ports;


public class ProtocolHandler_XID : ICustomInputDevice
{


    public void ReadData(Action<AnswerFromParticipant> keyPressedAction, Action<AnswerFromParticipant> keyReleasedAction)
    {
        try
        {
            int bytesToRead = _serialPort.BytesToRead;                      // Read the number of bytes that are ready to be read from the serial buffer
            if (bytesToRead < _bytesInData_ProtocolXID) return;                                   // Exit if nothing getted

            byte[] buffer = new byte[_bytesInData_ProtocolXID];                          // Create a buffer array to hold the incoming bytes
            _serialPort.Read(buffer, 0, _bytesInData_ProtocolXID);                       // Read the available bytes from the serial port into the buffer

            _dataQueue.Enqueue(ParseData_ProtocolXID(buffer));  // temp
            //_dataQueue.Enqueue(ByteArrayToBitString(buffer));  // temp
            //_dataQueue.Enqueue(Encoding.ASCII.GetString(buffer));           // the data transfer protocol is defined by the pin settings on the device itself
        }
        catch
        {
            stateTracker.UpdateSubState(AnswerDevice_Statuses.isConnected, false);              // May occure if "CheckPortConnection" didn't check yet, but "ReadData" was called
        }
    }
}

public class BitMask
{
    private ulong _mask;
    private int _offset;

    public BitMask(string mask)
    {
        if (mask.Length > 64)
            throw new ArgumentException("Max size for mask: ulong (64 bits)");

        var interimString = mask.Replace(" ", "");
        _mask = Convert.ToUInt64(interimString, 2);
        _offset = interimString.Length - interimString.LastIndexOf('1') - 1;

        if (_offset == interimString.Length)
            throw new ArgumentException("Max size for mask: ulong (64 bits)");
    }

    public ulong Apply(ulong data)
    {
        return (_mask & data) >> _offset;
    }

    public ulong Apply(string data)
    {
        return Apply(Convert.ToUInt64(data.Replace(" ", ""), 2));
    }
}*/

using System;

public class BitMask
{
    private ulong _mask;
    private int _offset;

    public BitMask(string mask)
    {
        if (mask.Length > 64)
            throw new ArgumentException("Max size for mask: ulong (64 bits)");

        var interimString = mask.Replace(" ", "");
        _mask = Convert.ToUInt64(interimString, 2);
        _offset = interimString.Length - interimString.LastIndexOf('1') - 1;

        if (_offset == interimString.Length)
            throw new ArgumentException("Max size for mask: ulong (64 bits)");
    }

    public ulong Apply(ulong data)
    {
        return (_mask & data) >> _offset;
    }

    public ulong Apply(string data)
    {
        return Apply(Convert.ToUInt64(data.Replace(" ", ""), 2));
    }
}