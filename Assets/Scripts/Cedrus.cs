using System.IO.Ports;
using System.Diagnostics;
using UnityEngine;
using System.Text;
using System.Collections;
using System;
using System.Collections.Concurrent;

public class Cedrus : MonoBehaviour
{
    //NOTES:
    // - because of data transfered in ASCII, dataLen % 8 = 0
    // - _serialPort.Open() -- is pretty heavy function. It takes about 4sec. But it shouldn't...
    // - GetCedrusPortName() -- is pretty heavy function too. It takes about 2sec


    private UiHandler _uiHandler;

    public bool CedrusIsConnected;

    private SerialPort _serialPort;
    private float _checkPortConnectionTimeInterval = 0.1f;  // sec
    //private float _checkPortNameTimeInterval = 3f;  // sec
    private int _checkPortConnectionReadTimeout = 1;        // ms   // todo: check if it can be less (the best is 0)
    private ConcurrentQueue<string> _dataQueue;
    private string _targetDisplayId;
    private string _portName;


    void OnEnable()
    {
        _uiHandler = GetComponent<UiHandler>();

        _dataQueue = new();
        _targetDisplayId = @"FTDIBUS\VID_0403+PID_6001";   // todo: move to config
        CedrusIsConnected = false;
        //_portName = GetCedrusPortName(_targetDisplayId);
        //if (_portName == null) StartCoroutine(CheckPortNumber());
    }

    void Update()
    {
        _uiHandler.PrintToInfo($"{CedrusIsConnected}", true);
        if (!CedrusIsConnected) _uiHandler.PrintToWarnings($"Cedrus. Check device connection and manualy try to reconnect", true);

        if (CedrusIsConnected) ReadDataFromBufer();
        if (_dataQueue.Count > 0)
        {
            while (_dataQueue.TryDequeue(out string data))
            {
                _uiHandler.PrintToWarnings($"Get from Cedrus: {data}. ", true);
            }
        }
    }

    void Start()
    {
        CedrusIsConnected = TryConnect();
        StartCoroutine(CheckPortConnection());
    }

    private bool TryConnect()
    {
        try
        {
            if (_serialPort != null && _serialPort.IsOpen && CedrusIsConnected == false)  // e.g port was opened but now it doesn work
            {
                _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;
                _portName = null;
            }


            _portName = GetCedrusPortName(_targetDisplayId);
            if (string.IsNullOrEmpty(_portName)) return false;

            _serialPort = new SerialPort(_portName);
            _serialPort.ReadTimeout = _checkPortConnectionReadTimeout;  // need only for port checking. doesn't affect reading when there are bytes to read

            _serialPort.BaudRate = 9600;            // BaudRate: The speed of data transmission, specifies how many bits of data are transmitted per second (bits per second)
            _serialPort.DataBits = 8;               // DataBits: The number of data bits in each data packet (usually 7 or 8, 8 in this case)
            _serialPort.Parity = Parity.None;       // Parity: A parity check method used to detect errors in the transmitted data (None means no parity check is used)
            _serialPort.StopBits = StopBits.One;    // StopBits: The number of stop bits used to indicate the end of a data packet (One means one stop bit)
            _serialPort.Handshake = Handshake.None; // Handshake: The flow control protocol (Handshake.None means no flow control is used)

            //_serialPort.DataReceived += ReadDataFromBufer;  // doesn't work in Unity

            _serialPort.DtrEnable = true;   // maybe it can help
            _serialPort.Open();

            return true;
        } catch //(Exception exceprion)
        {
            //_uiHandler.PrintToWarnings(exceprion.ToString(), true);
            return false;
        }
    }



    /// <summary>
    /// Calls external file to get port Cedrus connected to (using libs that are not allowed in Unity)
    /// </summary>
    /// <param name="deviceId">Device ID is common for all devices of same type. So it serves for all Cedrus boxes</param>
    /// <returns></returns>
    private string GetCedrusPortName(string deviceId)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "Assets/ExternalTools/PortFinder.exe",
            Arguments = deviceId,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        Process process = new Process { StartInfo = startInfo };
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return output;
    }

    private void ReadDataFromBufer()
    {
        try
        {
            int bytesToRead = _serialPort.BytesToRead;              // Read the number of bytes that are ready to be read from the serial buffer
            if (bytesToRead == 0) return;                           // Exit if nothing getted

            byte[] buffer = new byte[bytesToRead];                  // Create a buffer array to hold the incoming bytes
            _serialPort.Read(buffer, 0, bytesToRead);               // Read the available bytes from the serial port into the buffer

            _dataQueue.Enqueue(Encoding.ASCII.GetString(buffer));   // todo: devide by bytes and add them separately (if 2 or more btns were pushed simultaneously)
        }
        catch
        {
            // May occure if "CheckPortConnection" didn't check yet, but "ReadDataFromBufer" was called
            CedrusIsConnected = false;
        }
    }

    IEnumerator CheckPortConnection()
    {
        while (true)
        {
            try
            {
                if (_serialPort.BytesToRead == 0) _serialPort.ReadLine();
            }
            catch (TimeoutException)
            {
                // It's ok
            }
            catch
            {
                //CedrusIsConnected = TryConnect();
                CedrusIsConnected = false;
            }

            yield return new WaitForSeconds(_checkPortConnectionTimeInterval);  // if "yield return null" -- would wait until text frame
        }
    }



    /*IEnumerator CheckPortNumber()
    {
        while (true)
        {
            if (CedrusIsConnected == true) yield break;

            try
            {
                _portName = GetCedrusPortName(_targetDisplayId);   // todo: make port checking via ui btn "manual port check"
                if (_portName != null) yield break;
            }
            catch
            {
                _uiHandler.PrintToWarnings($"Cedrus. Fatal error in checking port name");
                yield break;
            }

            yield return new WaitForSeconds(_checkPortNameTimeInterval);  // if "yield return null" -- would wait until text frame
        }
    }*/

}
