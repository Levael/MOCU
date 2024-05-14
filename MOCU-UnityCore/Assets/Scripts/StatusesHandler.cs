using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class StatusesHandler : MonoBehaviour
{
    private Cedrus _cedrus;
    private UiHandler _uiHandler;
    private InputHandler _inputHandler;
    private AudioHandler _audioHandler;
    private ExperimentTabHandler _experimentTabHandler;

    private Dictionary<string, (StateTracker stateTracker, VisualElement visualElement)> _deviceNameToUxmlBlockMap;
    private Dictionary<StateTracker.DeviceConnectionStatus, string> _deviceConnectionStatusToColorMap;



    void Awake()
    {
        _cedrus = GetComponent<Cedrus>();
        _uiHandler = GetComponent<UiHandler>();
        _inputHandler = GetComponent<InputHandler>();
        _audioHandler = GetComponent<AudioHandler>();
        _experimentTabHandler = GetComponent<ExperimentTabHandler>();
    }

    void Start()
    {
        // don't move it to "Awake", otherwise ".mainUiScreen.GetElement("moog-status-block")" can be inaccessible

        _deviceNameToUxmlBlockMap = new()
        {
            //{ "Moog",       (stateTracker: _inputHandler.XRConnectionStatus, visualElement: _uiHandler.mainUiScreen.GetElement("gamepad-status-block"))_uiHandler.mainUiScreen.GetElement("moog-status-block") },
            { "Oculus",     (stateTracker: _inputHandler.XRConnectionStatus, visualElement: _uiHandler.mainUiScreen.GetElement("oculus-status-block")) },
            { "Cedrus",     (stateTracker: _cedrus.stateTracker, visualElement: _uiHandler.mainUiScreen.GetElement("cedrus-status-block")) },
            { "Gamepad",    (stateTracker: _inputHandler.GamepadConnectionStatus, visualElement: _uiHandler.mainUiScreen.GetElement("gamepad-status-block")) },
            { "Audio",      (stateTracker: _audioHandler.stateTracker, visualElement: _uiHandler.mainUiScreen.GetElement("audio-status-block")) },
            //{ "EEG",        _uiHandler.mainUiScreen.GetElement("eeg-status-block") },
            //{ "Trials",     _uiHandler.mainUiScreen.GetElement("trials-status-block") },
            //{ "Running",    _uiHandler.mainUiScreen.GetElement("running-status-block") }
        };

        _deviceConnectionStatusToColorMap = new()
        {                                                                       // todo: try to read vars from uss (currently not possible)
            { StateTracker.DeviceConnectionStatus.Connected,     "#5580ED" },   // blue
            { StateTracker.DeviceConnectionStatus.Disconnected,  "#FC5858" },   // red
            { StateTracker.DeviceConnectionStatus.InProgress,    "#FCBA58" },   // yellow
            { StateTracker.DeviceConnectionStatus.NotRelevant,   "#9A9B9B" }    // gray
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
                _experimentTabHandler.PrintToWarnings("Couldn't translate color");
            }
        }
    }
}




/*/// <summary>
/// Tracks the connection status of a device or a process based on multiple sub-states.
/// The overall connection status is determined by individual sub-state values where:
/// - If all sub-states are false, the overall status is set to Disconnected.
/// - If at least one sub-state is false (and not all are false), the overall status is set to InProgress.
/// - If all sub-states are true, the overall status is set to Connected.
/// This class is useful for managing complex states where multiple conditions contribute to the final connection status.
/// </summary>*/
public class StateTracker
{
    private Dictionary<string, bool?> _subStates;
    // 'status' here is used for final result (DeviceConnectionStatus), and 'state' -- for boolean values
    public DeviceConnectionStatus Status { get; private set; }


    public StateTracker(IEnumerable<string> subStateNames)
    {
        Status = DeviceConnectionStatus.NotRelevant;

        _subStates = new();
        foreach (var subStateName in subStateNames)
        {
            _subStates.Add(subStateName, null); 
        }
    }


    public void UpdateSubState(string subStateName, bool? subStateValue)
    {
        if (!_subStates.ContainsKey(subStateName))
        {
            UnityEngine.Debug.LogError("error in 'SetParameterState'");
            return;
        }

        if (_subStates[subStateName] != subStateValue)
        {
            _subStates[subStateName] = subStateValue;
            RecalculateOverallStatus();
        }
    }

    private void RecalculateOverallStatus()
    {
        if (_subStates.Values.All(state => state == null))
        {
            SetStatus(DeviceConnectionStatus.NotRelevant);
        }
        else if (_subStates.Values.Any(state => state == false))
        {
            SetStatus(DeviceConnectionStatus.Disconnected);
        }
        else if (_subStates.Values.All(state => state == true))
        {
            SetStatus(DeviceConnectionStatus.Connected);
        }
        else
        {
            SetStatus(DeviceConnectionStatus.InProgress);
        }
    }

    private void SetStatus(DeviceConnectionStatus newStatus)
    {
        if (Status != newStatus)
        {
            Status = newStatus;
        }
    }

    public enum DeviceConnectionStatus
    {
        Connected,
        Disconnected,
        InProgress,
        NotRelevant
    }
}