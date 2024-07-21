using System.IO.Ports;
using System.Diagnostics;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

// 0001 = XID 9600 (best)
// 0011 = ASCII 9600 (simplest)

/// <summary>
/// Make sure that the pins on the device itself are set as follows: 1(down) 2(down) 3(up) 4(up)
/// It configures Cedrus_old to work with ASCII encoding with baud rate 9600 (bits per second)
/// Cedrus_old documentation about it: https://cedrus.com/support/rb_series/tn1544_dip_switches.htm
/// XID commands: https://www.cedrus.com/support/xid/commands.htm
/// </summary>
public class Cedrus_old : MonoBehaviour
{
    public StateTracker stateTracker;
    //public DeviceConnection_Statuses CedrusConnectionStatus;

    public event Action<AnswerFromParticipant> gotData;     // calls every subscribed to it functions if got any data from participant (checks buffer every frame)

    private UiHandler _uiHandler;
    private ExperimentTabHandler _experimentTabHandler;

    private SerialPort _serialPort;
    private int _checkPortConnectionReadTimeout = 0;        // ms. If there will be any issue -- change to 1
    private ConcurrentQueue<string> _dataQueue;             // todo: rename it later
    private string _targetDeviceId;
    private string _portName;

    private Dictionary<string, AnswerFromParticipant> _codes_ProtocolASCII;
    private Dictionary<int, AnswerFromParticipant> _codes_ProtocolXID;
    private Dictionary<string, BitMask> _masks_ProtocolXID;
    private Dictionary<AnswerFromParticipant, int> _answerState;

    private long _tempTimeStamp = 0;
    private int _bytesInData_ProtocolXID = 6;


    private 


    void Awake()
    {
        _uiHandler = GetComponent<UiHandler>();
        _experimentTabHandler = GetComponent<ExperimentTabHandler>();

        _dataQueue = new();
        _targetDeviceId = @"FTDIBUS\VID_0403+PID_6001";   // todo: move to config
        stateTracker = new StateTracker(typeof(AnswerDevice_Statuses));

        _codes_ProtocolASCII = new() {
            { "a", AnswerFromParticipant.Up     },
            { "b", AnswerFromParticipant.Left   },
            { "c", AnswerFromParticipant.Center },
            { "d", AnswerFromParticipant.Right  },
            { "e", AnswerFromParticipant.Down   }
        };

        _codes_ProtocolXID = new()
        {
            { 1, AnswerFromParticipant.Up       },
            { 3, AnswerFromParticipant.Left     },
            { 4, AnswerFromParticipant.Center   },
            { 5, AnswerFromParticipant.Right    },
            { 6, AnswerFromParticipant.Down     }
        };

        _masks_ProtocolXID = new()
        {
            { "character",  new BitMask("00000000 00000000 00000000 00000000 00000000 11111111") },    // always 'k'
            { "port",       new BitMask("00000000 00000000 00000000 00000000 00001111 00000000") },    // always 0 (0 for keys, 3 for light)
            { "actionFlag", new BitMask("00000000 00000000 00000000 00000000 00010000 00000000") },    // 1-pressed, 0-released
            { "keyCode",    new BitMask("00000000 00000000 00000000 00000000 11100000 00000000") },    // actual answer
            { "timestamp",  new BitMask("11111111 11111111 11111111 11111111 00000000 00000000") }     // from cedrus' inner timer
        };

        _answerState = new()
        {
            { AnswerFromParticipant.Up,     0 },
            { AnswerFromParticipant.Left,   0 },
            { AnswerFromParticipant.Center, 0 },
            { AnswerFromParticipant.Right,  0 },
            { AnswerFromParticipant.Down,   0 },
        };
    }

    void Start() {}

    void OnDestroy()
    {
        if (_serialPort != null)
        {
            _serialPort.Close();
            _serialPort.Dispose();
            _serialPort = null;
            _portName = null;
        }
    }

    void Update()
    {
        if (stateTracker.Status == DeviceConnection_Statuses.Connected)
            ReadData();
        else
            return;

        
        if (_dataQueue.Count > 0)
        {
            while (_dataQueue.TryDequeue(out string data))                                      // the loop is needed in case several clicks were made in one frame
            {
                // Translates Cedrus_old ASCII string to "AnswerFromParticipant" enum type

                var signalFromParticipant = AnswerFromParticipant.Error;                        // ideally, this will not happen at all, it's needed as a stub

                if (_codes_ProtocolASCII.ContainsKey(data))                     // the usual scenario when everything is ok 
                    signalFromParticipant = _codes_ProtocolASCII[data];
                else if (data.Length > 1)                                                       // if several buttons were pressed simultaneously
                    signalFromParticipant = AnswerFromParticipant.MultipleAnswer;
                                        // trigger "InputHandler" function
                //gotData?.Invoke(signalFromParticipant);                                         // trigger "InputHandler" function

                // temp
                //print(_portName);
                print(data);

                if (_tempTimeStamp == 0)
                    _tempTimeStamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();
                else
                {
                    print($"got data. unity time: {((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds() - _tempTimeStamp}");
                    _tempTimeStamp = 0;
                }
                //print("data");
            }
        }
    }
    



    public void TryConnect(bool doRequestPortName)
    {
        // SOME NOTES:
        // serialPort.Open() -- is pretty heavy function. It takes about 4sec. But it shouldn't...
        // GetCedrusPortName() -- is pretty heavy function too. It takes about 2sec
        //_serialPort.DataReceived += ReadData;    // doesn't work in Unity (because of thread system), although it was the best option

        try
        {
            if (doRequestPortName) _portName = GetCedrusPortName(_targetDeviceId);

            if (string.IsNullOrEmpty(_portName))
            {
                _experimentTabHandler.PrintToWarnings($"doRequestPortName: {doRequestPortName}. _portName: {_portName}");
                stateTracker.UpdateSubState(AnswerDevice_Statuses.isConnected, false);
            }

            _serialPort = new SerialPort(_portName)
            {
                ReadTimeout = _checkPortConnectionReadTimeout,  // Is needed only for port checking. doesn't affect reading when there are bytes to read
                BaudRate = 9600,                                // The speed of data transmission, specifies how many bits of data are transmitted per second (bits per second)
                DataBits = 8,                                   // The number of data bits in each data packet (usually 7 or 8, 8 in this case)
                Parity = Parity.None,                           // A parity check method used to detect errors in the transmitted data (None means no parity check is used)
                StopBits = StopBits.One,                        // The number of stop bits used to indicate the end of a data packet (One means one stop bit)
                Handshake = Handshake.None,                     // The flow control protocol (Handshake.None means no flow control is used)
                DtrEnable = true                                // Maybe it can help to improve connection speed...
            };
            

            _serialPort.Open();
            stateTracker.UpdateSubState(AnswerDevice_Statuses.isConnected, true);

        } catch (Exception ex) {
            _experimentTabHandler.PrintToWarnings($"\n{ex}\n");
            stateTracker.UpdateSubState(AnswerDevice_Statuses.isConnected, false);
        }
    }



    /// <summary>
    /// Calls external file to get port Cedrus_old connected to (using libs that are not allowed in Unity)
    /// </summary>
    /// <param name="deviceId">Device ID is common for all devices of same type. So it serves for all Cedrus_old boxes</param>
    /// <returns></returns>
    private string GetCedrusPortName(string deviceId)
    {
        try {
            string relativeExternalAppPath = "Daemons/PortFinder.exe";
            string fullExternalAppPath = System.IO.Path.Combine(Application.streamingAssetsPath, relativeExternalAppPath);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fullExternalAppPath,
                Arguments = deviceId,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            Process process = new Process { StartInfo = startInfo };
            process.Start();

            string output = process.StandardOutput.ReadToEnd();                 // Get data from "Console.Write()"
            process.WaitForExit();

            return output;
        } catch (Exception ex) {
            _experimentTabHandler.PrintToWarnings($"\n{ex}\n");
            return null;
        }
    }

    /// <summary>
    /// Tries read data from port buffer to "_dataQueue". If fails -- then device was disconnected and so the connection status is updated
    /// </summary>
    private void ReadData()
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

    // todo: remove later -- InputHandler will take responsobility on that
    /// <summary>
    /// Coroutine that checks every "_checkPortConnectionTimeInterval" seconds if device is still connected and if not -- updates "CedrusConnectionStatus"
    /// </summary>
    public IEnumerator CheckPortConnection(float checkConnectionTimeInterval)
    {
        while (true)
        {
            try                         { if (_serialPort.BytesToRead == 0) _serialPort.ReadLine(); }       // on purpose tries cause an exception
            catch (TimeoutException)    { /* Do nothing */ }                                                // Device is connected but didn't send anything, it's ok
            catch                       { stateTracker.UpdateSubState(AnswerDevice_Statuses.isConnected, false); }              // if not "TimeoutException"-- device disconected

            yield return new WaitForSeconds(checkConnectionTimeInterval);                                   // if "yield return null" -- would wait until text frame
        }
    }


    private ulong GetRawData_BigEndianNotation(byte[] byteArray)
    {
        ulong value = 0;

        for (int i = 0; i < byteArray.Length; i++)
            value |= (ulong)byteArray[i] << (i * 8);

        return value;
    }

    private ulong GetRawData_AsNumber(byte[] byteArray)
    {
        Array.Resize(ref byteArray, 8);
        Array.Reverse(byteArray);
        return BitConverter.ToUInt64(byteArray, 0);
    }

    private string ParseData_ProtocolXID(byte[] byteArray)
    {
        // program can work for ~50 days (2^48 milliseconds)

        ulong rawData = GetRawData_BigEndianNotation(byteArray);

        short keyCode = (short)_masks_ProtocolXID["keyCode"].Apply(rawData);
        int timestamp = (int)_masks_ProtocolXID["timestamp"].Apply(rawData);
        bool actionFlag = _masks_ProtocolXID["actionFlag"].Apply(rawData) == 1;
        //char character = (char)_masks_ProtocolXID["character"].Apply(rawData);
        //int port = (int)_masks_ProtocolXID["port"].Apply(rawData);

        if (actionFlag)
            KeyPressed(keyCode, timestamp);
        else
            KeyReleased(keyCode, timestamp);

        return $"done";
        //return $"character: {character}, port: {port}, actionFlag: {actionFlag}, keyCode: {keyCode}, timestamp: {timestamp}\nraw message: {GetRawBinaryDataAsString(rawData)}"; //character: { character}, port: { port}, 
    }

    private void KeyPressed(short keyCode, int timestamp)
    {
        _answerState[_codes_ProtocolXID[keyCode]] = timestamp;
        print($"pressed {keyCode}");
    }
    private void KeyReleased(short keyCode, int timestamp)
    {
        var timePassed = timestamp - _answerState[_codes_ProtocolXID[keyCode]];
        _answerState[_codes_ProtocolXID[keyCode]] = 0;
        print($"released {keyCode}. time took {timePassed}ms");
    }

    private class ParsedData
    {
        public readonly char? character;
        public readonly int? port;
        public readonly bool isKeyPressed;
        public readonly bool isKeyReleased;
        public readonly int timestampMs;
        public readonly AnswerFromParticipant answer;

        public ParsedData(bool isKeyPressed, bool isKeyReleased, int timestampMs, AnswerFromParticipant answer, char? character = null, int? port = null)
        {
            this.character = character;
            this.port = port;
            this.isKeyPressed = isKeyPressed;
            this.isKeyReleased = isKeyReleased;
            this.timestampMs = timestampMs;
            this.answer = answer;
        }
    }
}
