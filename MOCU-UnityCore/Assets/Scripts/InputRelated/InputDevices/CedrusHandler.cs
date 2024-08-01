/*
    CedrusHandler.cs

    This class manages the interaction with the Cedrus input device, simulating keyboard input based on the device's data.
    It relies on a few key components to achieve its functionality:
    
    1. KeyboardSimulator.cs:
       - This class is responsible for creating and managing a virtual keyboard within Unity's Input System.
       - It allows the simulation of key presses, which can be used to translate Cedrus device inputs into keyboard actions.
       - The `SimulateKeyPress(Key key, bool isPressed)` method is used to simulate the pressing or releasing of a key (simulates NumPad).

    2. SerialPortHelper.cs:
       - Handles the setup and management of the serial port connection to the Cedrus device.
       - It includes methods for connecting to the device (`Connect()`), reading data (`ReadData(int numberOfBytes)`), 
         and checking the connection status (`CheckPortConnection()`).
       - The class ensures that the serial port is properly opened and closed, managing the lifecycle of the connection.

    3. XID_ProtocolParser.cs:
       - Parses data packets received from the Cedrus device using the XID protocol.
       - The `GetParsedData(byte[] byteArray)` method extracts relevant information such as the key code and action flag (pressed/released).
       - It uses bitmasking techniques to interpret the raw data from the device into usable input actions.

    Key Functionalities:
    - The `CedrusHandler` initializes and manages these components, ensuring that data from the Cedrus device is correctly interpreted and
      transformed into simulated keyboard inputs.
    - The `TryConnect()` method attempts to establish a connection with the device, first trying a quick connection and then attempting
      a more thorough connection if necessary.
    - The `CheckConnection(float checkConnectionTimeInterval)` coroutine periodically checks the connection status to ensure the device is still connected.
    - The `HandleData(byte[] byteArray)` method processes the data received from the device, using the `XID_ProtocolParser` to interpret it and the
      `KeyboardSimulator` to simulate the corresponding key presses.

    Notes:
    - The class is designed to work with a specific Cedrus device using the XID protocol and assumes a specific mapping from Cedrus key codes to keyboard keys.
    - The device has four pins that determine the data transmission speed and protocol. The correct settings are: DOWN, DOWN, DOWN, UP.
    - Cedrus switches documentation: https://cedrus.com/support/rb_series/tn1544_dip_switches.htm
    - XID protocol documentation: https://www.cedrus.com/support/xid/commands.htm

    Overall, the CedrusHandler class integrates the functionality of several components to enable the seamless use of a Cedrus device as an input method in Unity,
    translating specialized hardware inputs into standard keyboard actions.
*/



using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;


public class CedrusHandler : MonoBehaviour
{
    public StateTracker stateTracker;

    private KeyboardSimulator _keyboardSimulator;
    private XID_ProtocolParser _XID_ProtocolHelper;
    private SerialPortHelper _serialPortHelper;

    private int _baudRate = 9600;
    private string _targetDeviceId = @"FTDIBUS\VID_0403+PID_6001";
    private string _portFinderAppPath = "Daemons/PortFinder.exe";
    private int _checkPortConnectionReadTimeout = 0;        // ms. If there will be any issue -- change to 1
    private Dictionary<int, Key> _cedrus2keyboardCodesMap;



    private void Awake()
    {
        stateTracker = new StateTracker(typeof(AnswerDevice_Statuses));

        _XID_ProtocolHelper = new();
        _keyboardSimulator = new();
        _serialPortHelper = new();

        _cedrus2keyboardCodesMap = new()
        {
            { 1, Key.Numpad8 }, // up
            { 3, Key.Numpad4 }, // left
            { 4, Key.Numpad5 }, // center
            { 5, Key.Numpad6 }, // right
            { 6, Key.Numpad2 }, // down
        };
    }

    private void Start()
    {
        _keyboardSimulator.Init("Cedrus");
        TryConnect();
    }

    private void Update()
    {
        if (stateTracker.Status != DeviceConnection_Statuses.Connected)
            return;

        var rawData = _serialPortHelper.ReadData(_XID_ProtocolHelper.bytesInDataPacket);

        if (rawData != null)
            HandleData(rawData);
    }



    public async void TryConnect()
    {
        // first attempt (fast; assuming that the port name already exists and it is correct)
        var isConnectedSuccessfully = _serialPortHelper.Connect();

        if (isConnectedSuccessfully)
        {
            stateTracker.UpdateSubState(AnswerDevice_Statuses.isConnected, true);
            return;
        }

        // seccond attempt (slow; request to WinAPI for the port name)
        _serialPortHelper.SetParameters(portName: await GetCedrusPortName(_targetDeviceId), baudRate: _baudRate, readTimeout: _checkPortConnectionReadTimeout);
        isConnectedSuccessfully = _serialPortHelper.Connect();

        if (isConnectedSuccessfully)
            stateTracker.UpdateSubState(AnswerDevice_Statuses.isConnected, true);
        else
        {
            stateTracker.UpdateSubState(AnswerDevice_Statuses.isConnected, false);
            _serialPortHelper.Close();
        }
    }

    public IEnumerator CheckConnection(float checkConnectionTimeInterval)
    {
        while (true)
        {
            var isConnectedSuccessfully = _serialPortHelper.CheckPortConnection();
            if (!isConnectedSuccessfully)
            {
                stateTracker.UpdateSubState(AnswerDevice_Statuses.isConnected, false);
                _serialPortHelper.Close();
            }
            yield return new WaitForSeconds(checkConnectionTimeInterval);
        }
    }



    private void HandleData(byte[] byteArray)
    {
        var (keyCode, flag) = _XID_ProtocolHelper.GetParsedData(byteArray);
        _keyboardSimulator.SimulateKeyPress(key: _cedrus2keyboardCodesMap[keyCode], isPressed: flag);
    }

    private async Task<string> GetCedrusPortName(string deviceId)
    {
        try
        {
            string fullExternalAppPath = System.IO.Path.Combine(Application.streamingAssetsPath, _portFinderAppPath);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fullExternalAppPath,
                Arguments = deviceId,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();

                if (!process.HasExited)
                    process.Kill();

                return output.Trim();
            }
        }
        catch (Exception ex)
        {
            print("Error in 'GetCedrusPortName'");
            return null;
        }
    }
}