using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ConnectionsStatusesHandler : MonoBehaviour
{
    public bool classIsReady = false;

    private Cedrus _cedrus;
    private UiHandler _uiHandler;
    private InputHandler _inputHandler;
    private AudioHandler _audioHandler;

    private Dictionary<string, VisualElement> _deviceNameToUxmlBlockMap;
    private Dictionary<DeviceConnectionStatus, string> _deviceConnectionStatusToColorMap;



    void Awake()
    {
        _cedrus = GetComponent<Cedrus>();
        _uiHandler = GetComponent<UiHandler>();
        _inputHandler = GetComponent<InputHandler>();
        _audioHandler = GetComponent<AudioHandler>();
    }

    void Start()
    {
        // those dictionaries are in Start func because otherwise "UpdateConnectionStatuses" called before their initialization
        _deviceNameToUxmlBlockMap = new()
        {
            { "Moog",       _uiHandler.mainTabScreen.GetElement("moog-status-block") },
            { "Oculus",     _uiHandler.mainTabScreen.GetElement("oculus-status-block") },
            { "Cedrus",     _uiHandler.mainTabScreen.GetElement("cedrus-status-block") },
            { "Gamepad",    _uiHandler.mainTabScreen.GetElement("gamepad-status-block") },
            { "Audio",      _uiHandler.mainTabScreen.GetElement("audio-status-block") },
            { "EEG",        _uiHandler.mainTabScreen.GetElement("eeg-status-block") },
            { "Trials",     _uiHandler.mainTabScreen.GetElement("trials-status-block") },
            { "Running",    _uiHandler.mainTabScreen.GetElement("running-status-block") }
        };

        _deviceConnectionStatusToColorMap = new()
        {                                                           // todo: try to read vars from uss (currently not possible)
            { DeviceConnectionStatus.Connected,     "#5580ED" },    // blue
            { DeviceConnectionStatus.Disconnected,  "#FC5858" },    // red
            { DeviceConnectionStatus.InProgress,    "#FCBA58" },    // yellow
            { DeviceConnectionStatus.NotRelevant,   "#9A9B9B" }     // gray
        };

        // todo: add here event listeners for ReConnection

        classIsReady = true;    // is needed to prevent access to fields/methods that have not yet been initialized
    }



    public void UpdateConnectionStatuses()
    {
        if (!classIsReady) return;

        UpdateStatusBox(deviceName: "Cedrus", deviceStatus: _cedrus.CedrusConnectionStatus);
        UpdateStatusBox(deviceName: "Gamepad", deviceStatus: _inputHandler.GamepadConnectionStatus);
        UpdateStatusBox(deviceName: "Oculus", deviceStatus: _inputHandler.XRConnectionStatus);
        UpdateStatusBox(deviceName: "Audio", deviceStatus: _audioHandler.audioPipeConnectionStatus);
    }

    private void UpdateStatusBox(string deviceName, DeviceConnectionStatus deviceStatus)
    {
        if (_deviceNameToUxmlBlockMap.TryGetValue(deviceName, out var statusBlock))
        {
            VisualElement statusBlockColor = statusBlock.Q<VisualElement>(className: "status-block-color");
            Color color;


            if (ColorUtility.TryParseHtmlString(_deviceConnectionStatusToColorMap[deviceStatus], out color))
            {
                statusBlockColor.style.backgroundColor = new StyleColor(color);
            }
            else { _uiHandler.PrintToWarnings("Couldn't translate color"); }
        }
    }
}
