using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class StatusesHandler : MonoBehaviour, IControllableComponent
{
    private CedrusHandler _cedrus;
    private UiHandler _uiHandler;
    private InputHandler _inputHandler;
    private AudioHandler _audioHandler;
    private ExperimentTabHandler _experimentTabHandler;

    private Dictionary<string, (StateTracker stateTracker, VisualElement visualElement)> _deviceNameToUxmlBlockMap;
    private Dictionary<DeviceConnection_Statuses, string> _deviceConnectionStatusToColorMap;

    public bool IsComponentReady { get; private set; }


    public void ControllableAwake()
    {
        _cedrus = GetComponent<CedrusHandler>();
        _uiHandler = GetComponent<UiHandler>();
        _inputHandler = GetComponent<InputHandler>();
        _audioHandler = GetComponent<AudioHandler>();
        _experimentTabHandler = GetComponent<ExperimentTabHandler>();
    }

    public void ControllableStart()
    {
        // don't move it to "Awake", otherwise ".mainUiScreen.GetElement("moog-status-block")" can be inaccessible

        _deviceNameToUxmlBlockMap = new()
        {
            //{ "Moog",       (stateTracker: _inputHandler.XRConnectionStatus, visualElement: _uiHandler.mainUiScreen.GetElement("gamepad-status-block"))_uiHandler.mainUiScreen.GetElement("moog-status-block") },
            
            { "Oculus",     (stateTracker: _inputHandler.XRConnectionStatus, visualElement: (VisualElement)_uiHandler.mainUiScreen.elements.experimentTab.statusesModule.vr) },
            { "Cedrus_old", (stateTracker: _cedrus.stateTracker, visualElement: (VisualElement)_uiHandler.mainUiScreen.elements.experimentTab.statusesModule.cedrus) },
            { "Gamepad",    (stateTracker: _inputHandler.GamepadConnectionStatus, visualElement: (VisualElement)_uiHandler.mainUiScreen.elements.experimentTab.statusesModule.gamepad) },
            { "Audio",      (stateTracker: _audioHandler.stateTracker, visualElement: (VisualElement)_uiHandler.mainUiScreen.elements.experimentTab.statusesModule.audio) },

            //{ "EEG",        _uiHandler.mainUiScreen.GetElement("eeg-status-block") },
            //{ "Trials",     _uiHandler.mainUiScreen.GetElement("trials-status-block") },
            //{ "Running",    _uiHandler.mainUiScreen.GetElement("running-status-block") }
        };

        _deviceConnectionStatusToColorMap = new()
        {
            { DeviceConnection_Statuses.Connected,     "#5580ED" },   // blue
            { DeviceConnection_Statuses.Disconnected,  "#FC5858" },   // red
            { DeviceConnection_Statuses.InProgress,    "#FCBA58" },   // yellow
            { DeviceConnection_Statuses.NotRelevant,   "#9A9B9B" }    // gray
        };

        IsComponentReady = true;
    }


    void Update()
    {
        if (!IsComponentReady) return;


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
    public DeviceConnection_Statuses Status { get; private set; }

    private Dictionary<Enum, bool?> _subStates;    // 'T' here is used for final result (DeviceConnection_Statuses), and 'state' -- for boolean values
    


    public StateTracker(Type statusesEnumType)
    {
        Status = DeviceConnection_Statuses.NotRelevant;

        _subStates = new();
        foreach (var subStateName in Enum.GetValues(statusesEnumType))
        {
            _subStates.Add((Enum)subStateName, null); 
        }
    }


    public void UpdateSubState(Enum subStateName, bool? subStateValue)
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
            SetStatus(DeviceConnection_Statuses.NotRelevant);
        }
        else if (_subStates.Values.Any(state => state == false))
        {
            SetStatus(DeviceConnection_Statuses.Disconnected);
        }
        else if (_subStates.Values.All(state => state == true))
        {
            SetStatus(DeviceConnection_Statuses.Connected);
        }
        else
        {
            SetStatus(DeviceConnection_Statuses.InProgress);
        }
    }

    private void SetStatus(DeviceConnection_Statuses newStatus)
    {
        if (Status != newStatus)
        {
            Status = newStatus;
        }
    }
}