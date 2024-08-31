using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.XR;
using UnityEngine.Video;
using UnityEngine.UIElements;
using System.Linq;

public class InputHandler : MonoBehaviour, IControllableInitiation
{
    public bool IsComponentReady {  get; private set; }

    private InputActionAsset _inputActions;
    private InputLogic      _inputLogic;
    private CedrusHandler   _cedrus;
    private UiHandler       _uiHandler;
    

    private Dictionary<string, (Action<InputAction.CallbackContext> OnPressed, Action<InputAction.CallbackContext> OnReleased)> _inputSystem_actionHandlers;
    private Dictionary<string, AnswerFromParticipant> _actionNameToSignalMap;


    public StateTracker GamepadConnectionStatus;
    public StateTracker XRConnectionStatus;

    private float _checkGamepadConnectionTimeInterval       = 0.1f; // sec
    private float _checkCedrusPortConnectionTimeInterval    = 0.1f; // sec
    private float _checkXRConnectionTimeInterval            = 0.1f; // sec

    //private UnityEngine.InputSystem.InputDevice virtualGamepad;


    public void ControllableAwake()
    {
        _inputActions = Resources.Load<InputActionAsset>("InputActions");    // name of file
        GamepadConnectionStatus = new StateTracker(typeof(AnswerDevice_Statuses));
        XRConnectionStatus = new StateTracker(typeof(VrHeadset_Statuses));


        // INPUT SYSTEM PART (gamepad, keyboard and other devices Unity support)
        // Dictionary stores ActionName (from InputActionAsset) and tuple with two handlers: on "press" event and on "release" event
        _inputSystem_actionHandlers = new()
        {
            // Part of "Intercom" action map
            { "Input",      (OnPressed: GotSignalFromInputIntercom,     OnReleased: InputIntercomButtonWasReleased)},
            { "Output",     (OnPressed: GotSignalFromOutputIntercom,    OnReleased: OutputIntercomButtonWasReleased)},

            // Part of "Controller" action map
            { "Left",       (OnPressed: GotSignalFromInputSystem,       OnReleased: InputSystemButtonWasReleased)},
            { "Right",      (OnPressed: GotSignalFromInputSystem,       OnReleased: InputSystemButtonWasReleased)},
            { "Up",         (OnPressed: GotSignalFromInputSystem,       OnReleased: InputSystemButtonWasReleased)},
            { "Down",       (OnPressed: GotSignalFromInputSystem,       OnReleased: InputSystemButtonWasReleased)},
            { "Center",     (OnPressed: GotSignalFromInputSystem,       OnReleased: InputSystemButtonWasReleased)}
        };

        _actionNameToSignalMap = new()
        {
            { "Left",   AnswerFromParticipant.Left },
            { "Right",  AnswerFromParticipant.Right },
            { "Up",     AnswerFromParticipant.Up },
            { "Down",   AnswerFromParticipant.Down },
            { "Center", AnswerFromParticipant.Center }
        };

        // activates every action from InputSystem and, if it's in dict, adds its handler
        foreach (var actionMap in _inputActions.actionMaps)
        {
            foreach (var action in actionMap.actions)
            {
                action.Enable();

                if (_inputSystem_actionHandlers.TryGetValue(action.name, out var handlers))
                {
                    action.performed += handlers.OnPressed;
                    action.canceled += handlers.OnReleased;
                }
            }
        }

    }


    public void ControllableStart()
    {
        _inputLogic = GetComponent<InputLogic>();
        _cedrus = GetComponent<CedrusHandler>();
        _uiHandler = GetComponent<UiHandler>();

        StartCoroutine(_cedrus.CheckConnection(_checkCedrusPortConnectionTimeInterval));
        StartCoroutine(CheckGamepadConnection(_checkGamepadConnectionTimeInterval));
        StartCoroutine(CheckXRConnection(_checkXRConnectionTimeInterval));

        /*var devices = InputSystem.devices;
        foreach (var device in devices)
        {
            Debug.Log($"Device: {device.displayName}, Type: {device.GetType().Name}, Name: {device.name}");
        }*/

        IsComponentReady = true;
    }


    //THE NESTING IN FOLLOWING FUNCTIONS MAY SEEM REDUNDANT, BUT LET IT BE JUST IN CASE


    // TODO: yes, it will be working by Ignoring
    private void GotSignalFromInputSystem(InputAction.CallbackContext context)
    {
        var devicesList = InputSystem.devices;

        // todo: work on it. Maye just ignore if it's not in the list of active devices
        var device = context.control.device;
        var deviceName = device.name;
        var deviceDisplayName = device.displayName;
        var deviceType = device.GetType();
        var deviceDescription = device.description;

        if (device == devicesList[1]) return;
        //print($"device: {device}\ndeviceName: {deviceName}\ndeviceDisplayName: {deviceDisplayName}\ndeviceType: {deviceType}\ndeviceDescription: {deviceDescription}\n\n");


        if (_actionNameToSignalMap.TryGetValue(context.action.name, out AnswerFromParticipant signalFromParticipant))
            _inputLogic.GotPressSignalFromInputSystem(signalFromParticipant);

    }
    private void InputSystemButtonWasReleased(InputAction.CallbackContext context)
    {
        if (_actionNameToSignalMap.TryGetValue(context.action.name, out AnswerFromParticipant signalFromParticipant))
            _inputLogic.GotReleaseSignalFromInputSystem(signalFromParticipant);
    }


    /// <summary>
    /// When participant calls
    /// </summary>
    private void GotSignalFromInputIntercom(InputAction.CallbackContext context)
    {
        _inputLogic.IntercomFromParticipantStarted();
    }
    private void InputIntercomButtonWasReleased(InputAction.CallbackContext context)
    {
        _inputLogic.IntercomFromParticipantStopped();
    }

    /// <summary>
    /// When researcher calls
    /// </summary>
    private void GotSignalFromOutputIntercom(InputAction.CallbackContext context)
    {
        _inputLogic.IntercomFromResearcherStarted();
    }
    private void OutputIntercomButtonWasReleased(InputAction.CallbackContext context)
    {
        _inputLogic.IntercomFromResearcherStopped();
    }




    // CONNECTION CHECKERS

    private IEnumerator CheckGamepadConnection(float checkConnectionTimeInterval)
    {
        while (true)
        {
            try
            {
                if (Gamepad.current?.enabled ?? false)
                    GamepadConnectionStatus.UpdateSubState(AnswerDevice_Statuses.isConnected, true);
                else
                    GamepadConnectionStatus.UpdateSubState(AnswerDevice_Statuses.isConnected, false);
            }
            catch
            {
                GamepadConnectionStatus.UpdateSubState(AnswerDevice_Statuses.isConnected, false);
            }

            yield return new WaitForSeconds(checkConnectionTimeInterval);
        }
    }

    private IEnumerator CheckXRConnection(float checkConnectionTimeInterval)
    {
        while (true)
        {
            try
            {
                List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
                SubsystemManager.GetInstances(subsystems);

                if (subsystems.Count != 1)
                {
                    XRConnectionStatus.UpdateSubState(VrHeadset_Statuses.isConnected, false);
                    XRConnectionStatus.UpdateSubState(VrHeadset_Statuses.iOnHead, false);
                }
                else if (subsystems[0].running && IsHeadsetWorn())
                {
                    XRConnectionStatus.UpdateSubState(VrHeadset_Statuses.isConnected, true);
                    XRConnectionStatus.UpdateSubState(VrHeadset_Statuses.iOnHead, true);
                }
                else
                {
                    XRConnectionStatus.UpdateSubState(VrHeadset_Statuses.isConnected, true);
                    XRConnectionStatus.UpdateSubState(VrHeadset_Statuses.iOnHead, null);     // 'null' and not 'false' because I need status to be yellow, not red (half working)
                }
            }
            catch
            {
                XRConnectionStatus.UpdateSubState(VrHeadset_Statuses.isConnected, false);
                XRConnectionStatus.UpdateSubState(VrHeadset_Statuses.iOnHead, false);

                Debug.LogError("Crash in 'CheckXRConnection'");
            }

            yield return new WaitForSeconds(checkConnectionTimeInterval);
        }
    }

    private bool IsHeadsetWorn()
    {
        List<XRNodeState> nodeStates = new List<XRNodeState>();
        InputTracking.GetNodeStates(nodeStates);

        foreach (var nodeState in nodeStates)
        {
            if (nodeState.nodeType == XRNode.Head)
            {
                return nodeState.tracked;
            }
        }

        return false;
    }
}
