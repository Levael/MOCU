using System.IO.Ports;
using System.Diagnostics;
using UnityEngine;
using System.Text;
using System.Collections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;


/// <summary>
/// Make sure that the pins on the device itself are set as follows: 1(down) 2(down) 3(up) 4(up)
/// It configures Cedrus to work with ASCII encoding with baud rate 9600 (bits per second)
/// Cedrus documentation about it: https://cedrus.com/support/rb_series/tn1544_dip_switches.htm
/// </summary>
public class Cedrus : MonoBehaviour
{
    public StateTracker stateTracker;
    //public DeviceConnection_Statuses CedrusConnectionStatus;

    public event Action<AnswerFromParticipant> gotData;     // calls every subscribed to it functions if got any data from participant (checks buffer every frame)

    private UiHandler _uiHandler;
    private ExperimentTabHandler _experimentTabHandler;

    private SerialPort _serialPort;
    //private float _checkPortConnectionTimeInterval = 0.1f;  // sec
    private int _checkPortConnectionReadTimeout = 0;        // ms. If there will be any issue -- change to 1
    private ConcurrentQueue<string> _dataQueue;             // todo: rename it later
    private string _targetDeviceId;
    private string _portName;

    private Dictionary<string, AnswerFromParticipant> _cedrusCodes_answerSignals_Relations;


    void Awake()
    {
        _uiHandler = GetComponent<UiHandler>();
        _experimentTabHandler = GetComponent<ExperimentTabHandler>();

        _dataQueue = new();
        _targetDeviceId = @"FTDIBUS\VID_0403+PID_6001";   // todo: move to config
        stateTracker = new StateTracker(typeof(AnswerDevice_Statuses));

        _cedrusCodes_answerSignals_Relations = new() {
            { "a", AnswerFromParticipant.Up     },
            { "b", AnswerFromParticipant.Left   },
            { "c", AnswerFromParticipant.Center },
            { "d", AnswerFromParticipant.Right  },
            { "e", AnswerFromParticipant.Down   }
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
        if (stateTracker.Status == DeviceConnection_Statuses.Connected) ReadDataFromBufer();    // checks por buffer every frame in case any new data
        
        if (_dataQueue.Count > 0)
        {
            while (_dataQueue.TryDequeue(out string data))                                      // the loop is needed in case several clicks were made in one frame
            {
                // Translates Cedrus ASCII string to "AnswerFromParticipant" enum type

                var signalFromParticipant = AnswerFromParticipant.Error;                        // ideally, this will not happen at all, it's needed as a stub

                if (_cedrusCodes_answerSignals_Relations.ContainsKey(data))                     // the usual scenario when everything is ok 
                    signalFromParticipant = _cedrusCodes_answerSignals_Relations[data];
                else if (data.Length > 1)                                                       // if several buttons were pressed simultaneously
                    signalFromParticipant = AnswerFromParticipant.MultipleAnswer;

                gotData?.Invoke(signalFromParticipant);                                         // trigger "InputHandler" function
            }
        }
    }
    



    public void TryConnect(bool doRequestPortName)
    {
        // SOME NOTES:
        // serialPort.Open() -- is pretty heavy function. It takes about 4sec. But it shouldn't...
        // GetCedrusPortName() -- is pretty heavy function too. It takes about 2sec
        //_serialPort.DataReceived += ReadDataFromBufer;    // doesn't work in Unity (because of thread system), although it was the best option

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
    /// Calls external file to get port Cedrus connected to (using libs that are not allowed in Unity)
    /// </summary>
    /// <param name="deviceId">Device ID is common for all devices of same type. So it serves for all Cedrus boxes</param>
    /// <returns></returns>
    private string GetCedrusPortName(string deviceId)
    {
        try {
            string relativeExternalAppPath = "PortFinder.exe";
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
    private void ReadDataFromBufer()
    {
        try
        {
            int bytesToRead = _serialPort.BytesToRead;                      // Read the number of bytes that are ready to be read from the serial buffer
            if (bytesToRead == 0) return;                                   // Exit if nothing getted

            byte[] buffer = new byte[bytesToRead];                          // Create a buffer array to hold the incoming bytes
            _serialPort.Read(buffer, 0, bytesToRead);                       // Read the available bytes from the serial port into the buffer

            _dataQueue.Enqueue(Encoding.ASCII.GetString(buffer));           // the data transfer protocol is defined by the pin settings on the device itself
        }
        catch
        {
            stateTracker.UpdateSubState(AnswerDevice_Statuses.isConnected, false);              // May occure if "CheckPortConnection" didn't check yet, but "ReadDataFromBufer" was called
        }
    }

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

}
