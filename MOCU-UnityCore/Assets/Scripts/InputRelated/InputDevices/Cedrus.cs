using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;


public class CedrusHandler : MonoBehaviour
{
    public StateTracker stateTracker;

    private SerialPort _serialPort;
    private string _portName;
    private int _baudRate = 9600;
    private string _targetDeviceId = @"FTDIBUS\VID_0403+PID_6001";
    private string _portFinderAppPath = "Daemons/PortFinder.exe";
    private int _checkPortConnectionReadTimeout = 0;        // ms. If there will be any issue -- change to 1

    private int _bytesInData_ProtocolXID = 6;
    private Dictionary<int, Key> _codes_ProtocolXID;
    private Dictionary<string, BitMask> _masks_ProtocolXID;

    private CedrusKeyboard _virtualKeyboard;
    private KeyboardState _keyboardState;


    private void Awake()
    {
        stateTracker = new StateTracker(typeof(AnswerDevice_Statuses));
        _keyboardState = new KeyboardState();


        _codes_ProtocolXID = new()
        {
            { 1, Key.Numpad8 }, // up
            { 3, Key.Numpad4 }, // left
            { 4, Key.Numpad5 }, // center
            { 5, Key.Numpad6 }, // right
            { 6, Key.Numpad2 }, // down
        };

        _masks_ProtocolXID = new()
        {
            { "character",  new BitMask("00000000 00000000 00000000 00000000 00000000 11111111") },    // always 'k'
            { "port",       new BitMask("00000000 00000000 00000000 00000000 00001111 00000000") },    // always 0 (0 for keys, 3 for light)
            { "actionFlag", new BitMask("00000000 00000000 00000000 00000000 00010000 00000000") },    // 1-pressed, 0-released
            { "keyCode",    new BitMask("00000000 00000000 00000000 00000000 11100000 00000000") },    // actual answer
            { "timestamp",  new BitMask("11111111 11111111 11111111 11111111 00000000 00000000") }     // from cedrus' inner timer
        };
    }

    private void Start()
    {
        _virtualKeyboard = InitDedvice();
        TryConnect();
    }

    private void Update()
    {
        if (stateTracker.Status == DeviceConnection_Statuses.Connected)
            ReadData();
        else
            return;
    }



    private CedrusKeyboard InitDedvice()
    {
        InputSystem.RegisterLayout<CedrusKeyboard>(
            matches: new InputDeviceMatcher()
                .WithInterface("Keyboard")
                .WithManufacturer("Custom")
                .WithProduct("Cedrus"));

        var virtualKeyboard = (CedrusKeyboard)InputSystem.devices.FirstOrDefault(device =>   
            device.description.product == "Cedrus" &&
            device.description.manufacturer == "Custom"
        );

        if (virtualKeyboard == null)
        {
            //virtualKeyboard = InputSystem.AddDevice<Keyboard>();
            virtualKeyboard = (CedrusKeyboard)InputSystem.AddDevice(new InputDeviceDescription
            {
                product = "Cedrus",
                manufacturer = "Custom",
                interfaceName = "Keyboard"
            });
        }

        return virtualKeyboard;
    }

    private void ReadData()
    {
        try
        {
            int bytesToRead = _serialPort.BytesToRead;
            if (bytesToRead < _bytesInData_ProtocolXID) return;

            byte[] buffer = new byte[_bytesInData_ProtocolXID];
            _serialPort.Read(buffer, 0, _bytesInData_ProtocolXID);

            HandleData(buffer);
        }
        catch (Exception ex)
        {
            //stateTracker.UpdateSubState(AnswerDevice_Statuses.isConnected, false);
            print($"error while 'ReadData': {ex}");
        }
    }

    private void HandleData(byte[] byteArray)
    {
        Array.Resize(ref byteArray, 8);
        var rawData = BitConverter.ToUInt64(byteArray, 0);

        short keyCode = (short)_masks_ProtocolXID["keyCode"].Apply(rawData);
        int timestamp = (int)_masks_ProtocolXID["timestamp"].Apply(rawData);
        bool actionFlag = _masks_ProtocolXID["actionFlag"].Apply(rawData) == 1;
        char character = (char)_masks_ProtocolXID["character"].Apply(rawData);
        int port = (int)_masks_ProtocolXID["port"].Apply(rawData);

        /*print($"" +
            $"character: {character}," +
            $"port: {port}," +
            $"actionFlag: {actionFlag}," +
            $"keyCode: {keyCode}," +
            $"timestamp: {timestamp}\n" +
            $"character: {character}," +
            $"port: {port}" +
            $"raw message: {Convert.ToString((long)rawData, 2).PadLeft(48, '0')},"
        );*/

        SimulateKeyPress(key: _codes_ProtocolXID[keyCode], isPressed: actionFlag);
    }

    private void SimulateKeyPress(Key key, bool isPressed)
    {
        if (isPressed)
            _keyboardState.Press(key);
        else
            _keyboardState.Release(key);

        InputSystem.QueueStateEvent(_virtualKeyboard, _keyboardState);
    }

    private string GetCedrusPortName(string deviceId)
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

            Process process = new Process { StartInfo = startInfo };
            process.Start();

            string output = process.StandardOutput.ReadToEnd();                 // Get data from "Console.Write()"
            process.WaitForExit();

            return output;
        }
        catch (Exception ex)
        {
            //_experimentTabHandler.PrintToWarnings($"\n{ex}\n");
            return null;
        }
    }

    public void TryConnect(bool doRequestPortName = true)
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
                //_experimentTabHandler.PrintToWarnings($"doRequestPortName: {doRequestPortName}. _portName: {_portName}");
                stateTracker.UpdateSubState(AnswerDevice_Statuses.isConnected, false);
            }

            _serialPort = new SerialPort(_portName)
            {
                ReadTimeout = _checkPortConnectionReadTimeout,  // Is needed only for port checking. doesn't affect reading when there are bytes to read
                BaudRate = _baudRate,                           // The speed of data transmission, specifies how many bits of data are transmitted per second (bits per second)
                DataBits = 8,                                   // The number of data bits in each data packet (usually 7 or 8, 8 in this case)
                Parity = Parity.None,                           // A parity check method used to detect errors in the transmitted data (None means no parity check is used)
                StopBits = StopBits.One,                        // The number of stop bits used to indicate the end of a data packet (One means one stop bit)
                Handshake = Handshake.None,                     // The flow control protocol (Handshake.None means no flow control is used)
                DtrEnable = true                                // Maybe it can help to improve connection speed...
            };


            _serialPort.Open();
            stateTracker.UpdateSubState(AnswerDevice_Statuses.isConnected, true);

        }
        catch (Exception ex)
        {
            //_experimentTabHandler.PrintToWarnings($"\n{ex}\n");
            stateTracker.UpdateSubState(AnswerDevice_Statuses.isConnected, false);
        }
    }
}

[InputControlLayout(stateType = typeof(KeyboardState))]
public class CedrusKeyboard : Keyboard
{
}

/*public class CedrusHandler : MonoBehaviour
{
    public StateTracker stateTracker;

    private SerialPort _serialPort;
    private string _portName;
    private int _baudRate = 9600;
    private string _targetDeviceId = @"FTDIBUS\VID_0403+PID_6001";
    private int _checkPortConnectionReadTimeout = 0;        // ms. If there will be any issue -- change to 1

    private CedrusDevice _device;
    private CedrusState _deviceState;

    private Dictionary<int, AnswerFromParticipant> _codes_ProtocolXID;

    void OnEnable()
    {
        InputSystem.RegisterLayout<CedrusDevice>();
        _device = InputSystem.AddDevice<CedrusDevice>();

        _codes_ProtocolXID = new()
        {
            { 1, AnswerFromParticipant.Up       },
            { 3, AnswerFromParticipant.Left     },
            { 4, AnswerFromParticipant.Center   },
            { 5, AnswerFromParticipant.Right    },
            { 6, AnswerFromParticipant.Down     }
        };

        TryConnect();
    }

    void OnDisable()
    {
        if (_device != null)
            InputSystem.RemoveDevice(_device);

        if (_serialPort != null && _serialPort.IsOpen)
            _serialPort.Close();
    }

    void Update()
    {
        if (_serialPort.IsOpen)
        {
            try
            {
                string data = serialPort.ReadLine();
                // Предположим, что данные представляют собой строку "left,right,up,down,center"
                string[] values = data.Split(',');
                _deviceState.leftButton = bool.Parse(values[0]);
                _deviceState.rightButton = bool.Parse(values[1]);
                _deviceState.upButton = bool.Parse(values[2]);
                _deviceState.downButton = bool.Parse(values[3]);
                _deviceState.centerButton = bool.Parse(values[4]);

                InputSystem.QueueStateEvent(device, state);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Error reading from COM port: " + ex.Message);
            }
        }
    }

    private async void TryConnect(bool doRequestPortName = true)
    {
        // SOME NOTES:
        // serialPort.Open() -- is pretty heavy function. It takes about 4sec. But it shouldn't...
        // GetCedrusPortName() -- is pretty heavy function too. It takes about 2sec
        //_serialPort.DataReceived += ReadData;    // doesn't work in Unity (because of thread system), although it was the best option

        try
        {
            if (doRequestPortName) _portName = await GetCedrusPortName(_targetDeviceId);

            if (string.IsNullOrEmpty(_portName))
            {
                //_experimentTabHandler.PrintToWarnings($"doRequestPortName: {doRequestPortName}. _portName: {_portName}");
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

        }
        catch (Exception ex)
        {
            //_experimentTabHandler.PrintToWarnings($"\n{ex}\n");
            stateTracker.UpdateSubState(AnswerDevice_Statuses.isConnected, false);
        }
    }

    /// <summary>
    /// Calls external file to get port Cedrus_old connected to (using libs that are not allowed in Unity)
    /// </summary>
    /// <param name="deviceId">Device ID is common for all devices of same type. So it serves for all Cedrus_old boxes</param>
    /// <returns></returns>
    private async Task<string> GetCedrusPortName(string deviceId)
    {
        try
        {
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

            string output = await process.StandardOutput.ReadToEndAsync();                 // Get data from "Console.Write()"
            await process.WaitForExitAsync();

            return output;
        }
        catch (Exception ex)
        {
            //_experimentTabHandler.PrintToWarnings($"\n{ex}\n");
            return null;
        }
    }
}*/


/*
 
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO.Ports;
using System;
using System.Collections.Generic;

public class CustomKeyboardHandler : MonoBehaviour
{
    private SerialPort serialPort;
    public string portName = "COM3";
    public int baudRate = 9600;
    private Keyboard virtualKeyboard;
    private HashSet<Key> pressedKeys = new HashSet<Key>();

    void Start()
    {
        EnableVirtualKeyboard();

        // Открываем SerialPort для чтения данных
        serialPort = new SerialPort(portName, baudRate);
        serialPort.Open();
    }

    void Update()
    {
        if (serialPort.IsOpen)
        {
            try
            {
                string data = serialPort.ReadLine();
                string[] values = data.Split(',');
                bool isKeyPressed = bool.Parse(values[0]);
                Key currentKey = (Key)Enum.Parse(typeof(Key), values[1]);

                if (isKeyPressed)
                {
                    pressedKeys.Add(currentKey);
                }
                else
                {
                    pressedKeys.Remove(currentKey);
                }

                SimulateKeyPress();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Error reading from COM port: " + ex.Message);
            }
        }
    }

    void OnDestroy()
    {
        DisableVirtualKeyboard();

        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }

    private void SimulateKeyPress()
    {
        var keyboardState = new KeyboardState();
        foreach (var key in pressedKeys)
        {
            keyboardState.WithKey(key);
        }
        InputSystem.QueueStateEvent(virtualKeyboard, keyboardState);
    }

    public void EnableVirtualKeyboard()
    {
        if (virtualKeyboard == null)
        {
            virtualKeyboard = InputSystem.AddDevice<Keyboard>();
            virtualKeyboard.description = new InputDeviceDescription
            {
                product = "Virtual Keyboard",
                manufacturer = "Custom"
            };
        }
    }

    public void DisableVirtualKeyboard()
    {
        if (virtualKeyboard != null)
        {
            InputSystem.RemoveDevice(virtualKeyboard);
            virtualKeyboard = null;
        }
    }
}

 
 */

/*
 using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ActionHandler : MonoBehaviour
{
    public InputActionAsset inputActions;
    private InputActionMap actionMap;
    private Dictionary<string, Action<InputAction.CallbackContext>> actionHandlers;

    void OnEnable()
    {
        actionMap = inputActions.FindActionMap("Controller");
        actionMap.Enable();

        actionHandlers = new Dictionary<string, Action<InputAction.CallbackContext>>()
        {
            { "Left", ctx => HandleAction("Left", ctx) },
            { "Right", ctx => HandleAction("Right", ctx) },
            { "Up", ctx => HandleAction("Up", ctx) },
            { "Down", ctx => HandleAction("Down", ctx) },
            { "Center", ctx => HandleAction("Center", ctx) }
        };

        foreach (var action in actionMap.actions)
        {
            if (actionHandlers.ContainsKey(action.name))
            {
                action.performed += actionHandlers[action.name];
                action.canceled += actionHandlers[action.name];
            }
        }
    }

    void OnDisable()
    {
        foreach (var action in actionMap.actions)
        {
            if (actionHandlers.ContainsKey(action.name))
            {
                action.performed -= actionHandlers[action.name];
                action.canceled -= actionHandlers[action.name];
            }
        }
        actionMap.Disable();
    }

    private void HandleAction(string actionName, InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log($"{actionName} Pressed");
        }
        else if (context.canceled)
        {
            Debug.Log($"{actionName} Released");
        }
    }
}

 */