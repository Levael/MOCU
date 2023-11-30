using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ConnectionsStatusesHandler : MonoBehaviour
{
    public bool classIsReady = false;

    private Cedrus _cedrus;
    private UiHandler _uiHandler;

    private Dictionary<string, VisualElement> _deviceNameToUxmlBlockMap;
    private Dictionary<DeviceConnectionStatus, string> _deviceConnectionStatusToColorMap;



    void Awake()
    {
        _cedrus = GetComponent<Cedrus>();
        _uiHandler = GetComponent<UiHandler>();
    }

    void Start()
    {
        _deviceNameToUxmlBlockMap = new()
        {
            { "Moog",       _uiHandler.mainScreen.GetElement("moog-status-block") },
            { "Oculus",     _uiHandler.mainScreen.GetElement("oculus-status-block") },
            { "Cedrus",     _uiHandler.mainScreen.GetElement("cedrus-status-block") },
            { "Gamepad",    _uiHandler.mainScreen.GetElement("gamepad-status-block") },
            { "Audio",      _uiHandler.mainScreen.GetElement("audio-status-block") },
            { "EEG",        _uiHandler.mainScreen.GetElement("eeg-status-block") },
            { "Trials",     _uiHandler.mainScreen.GetElement("trials-status-block") },
            { "Running",    _uiHandler.mainScreen.GetElement("running-status-block") }
        };

        _deviceConnectionStatusToColorMap = new()
        {                                                           // todo: try to read vars from uss (currently not possible)
            { DeviceConnectionStatus.Connected,     "#5580ED" },    // blue
            { DeviceConnectionStatus.Disconnected,  "#FC5858" },    // red
            { DeviceConnectionStatus.InProgress,    "#FCBA58" },    // yellow
            { DeviceConnectionStatus.NotRelevant,   "#9A9B9B" }     // gray
        };

        classIsReady = true;
    }



    public void UpdateConnectionStatuses()
    {
        if (!classIsReady) return;

        UpdateStatusBox(deviceName: "Cedrus", deviceStatus: _cedrus.CedrusConnectionStatus);
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
