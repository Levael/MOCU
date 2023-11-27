using System.IO.Ports;
using System.Diagnostics;
using UnityEngine;
using System.Text;

public class Cedrus : MonoBehaviour
{
    private UiHandler _uiHandler;
    private SerialPort _serialPort;


    void OnEnable()
    {
        _uiHandler = GetComponent<UiHandler>();
    }

    void OnDisable()
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.Close();
            _serialPort.Dispose();
        }
    }

    void Update()
    {
        //_uiHandler.PrintToInfo($"Cedrus Is open: {_serialPort.IsOpen}");
    }

    public void InitCedrus()
    {
        var deviceId = @"FTDIBUS\VID_0403+PID_6001";   // todo: move to config
        string portName = GetCedrusPortName(deviceId);

        if (string.IsNullOrEmpty(portName))
        {
            _uiHandler.PrintToWarnings("Cedrus not found");
            return;
        }

        _serialPort = new SerialPort(portName);
        
        _serialPort.BaudRate = 9600;             // BaudRate: The speed of data transmission, specifies how many bits of data are transmitted per second (bits per second)
        _serialPort.DataBits = 8;                // DataBits: The number of data bits in each data packet (usually 7 or 8, 8 in this case)
        _serialPort.Parity = Parity.None;      // Parity: A parity check method used to detect errors in the transmitted data (None means no parity check is used)
        _serialPort.StopBits = StopBits.One;     // StopBits: The number of stop bits used to indicate the end of a data packet (One means one stop bit)
        _serialPort.Handshake = Handshake.None;   // Handshake: The flow control protocol (Handshake.None means no flow control is used)

        _serialPort.DataReceived += SerialPort_DataReceived;

        _serialPort.Open();

        _uiHandler.PrintToInfo($"Cedrus connected to port: {portName}. Is open: {_serialPort.IsOpen}");
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

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        _uiHandler.PrintToInfo("O_o");

        SerialPort serialPort = (SerialPort)sender;             // Cast the sender to a SerialPort object to access its methods and properties
        int bytesToRead = serialPort.BytesToRead;               // Read the number of bytes that are ready to be read from the serial buffer

        byte[] buffer = new byte[bytesToRead];                  // Create a buffer array to hold the incoming bytes
        serialPort.Read(buffer, 0, bytesToRead);                // Read the available bytes from the serial port into the buffer

        _uiHandler.PrintToInfo($"Get from Cedrus: {Encoding.ASCII.GetString(buffer)}");
    }
}
