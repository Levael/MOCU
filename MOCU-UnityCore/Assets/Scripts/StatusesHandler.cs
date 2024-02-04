using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StatusesHandler : MonoBehaviour
{
    private Cedrus _cedrus;
    private UiHandler _uiHandler;
    private InputHandler _inputHandler;
    private AudioHandler _audioHandler;

    private Dictionary<string, (StateTracker stateTracker, VisualElement visualElement)> _deviceNameToUxmlBlockMap;
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
        // don't move it to "Awake", otherwise ".mainTabScreen.GetElement("moog-status-block")" can be inaccessible

        _deviceNameToUxmlBlockMap = new()
        {
            //{ "Moog",       (stateTracker: _inputHandler.XRConnectionStatus, visualElement: _uiHandler.mainTabScreen.GetElement("gamepad-status-block"))_uiHandler.mainTabScreen.GetElement("moog-status-block") },
            { "Oculus",     (stateTracker: _inputHandler.XRConnectionStatus, visualElement: _uiHandler.mainTabScreen.GetElement("oculus-status-block")) },
            { "Cedrus",     (stateTracker: _cedrus.stateTracker, visualElement: _uiHandler.mainTabScreen.GetElement("cedrus-status-block")) },
            { "Gamepad",    (stateTracker: _inputHandler.XRConnectionStatus, visualElement: _uiHandler.mainTabScreen.GetElement("gamepad-status-block")) },
            { "Audio",      (stateTracker: _audioHandler.stateTracker, visualElement: _uiHandler.mainTabScreen.GetElement("audio-status-block")) },
            //{ "EEG",        _uiHandler.mainTabScreen.GetElement("eeg-status-block") },
            //{ "Trials",     _uiHandler.mainTabScreen.GetElement("trials-status-block") },
            //{ "Running",    _uiHandler.mainTabScreen.GetElement("running-status-block") }
        };

        _deviceConnectionStatusToColorMap = new()
        {                                                           // todo: try to read vars from uss (currently not possible)
            { DeviceConnectionStatus.Connected,     "#5580ED" },    // blue
            { DeviceConnectionStatus.Disconnected,  "#FC5858" },    // red
            { DeviceConnectionStatus.InProgress,    "#FCBA58" },    // yellow
            { DeviceConnectionStatus.NotRelevant,   "#9A9B9B" }     // gray
        };
    }

    void Update()
    {
        // change later to: "update when changed" or "update with a lower frequency"

        foreach (var device in _deviceNameToUxmlBlockMap)
        {
            UpdateStatusBox(device.Key);
        }
    }



    private void UpdateStatusBox(string deviceName)
    {
        if (_deviceNameToUxmlBlockMap.TryGetValue(deviceName, out var paramsTuple))
        {
            VisualElement statusBlockColor = paramsTuple.visualElement.Q<VisualElement>(className: "status-block-color");

            if (ColorUtility.TryParseHtmlString(_deviceConnectionStatusToColorMap[paramsTuple.stateTracker.Status], out Color color))
            {
                statusBlockColor.style.backgroundColor = new StyleColor(color);
            }
            else
            {
                _uiHandler.PrintToWarnings("Couldn't translate color");
            }
        }
    }
}



// temp here
public class StateTracker {
    public DeviceConnectionStatus Status { get; private set; }

    public StateTracker(DeviceConnectionStatus initialStatus)
    {
        Status = initialStatus;
    }

    public void SetStatus(DeviceConnectionStatus newStatus)
    {
        if (Status != newStatus)
        {
            Status = newStatus;
        }
    }
}

public enum DeviceConnectionStatus
{
    Connected,
    Disconnected,
    InProgress,
    NotRelevant
}
