using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class InputHandler : ManagedMonoBehaviour
{
    private InputActionAsset _inputActions;
    private InputLogic      _inputLogic;
    //private CedrusHandler   _cedrus;
    private UiHandler       _ui;
    private ControllersHandler _controller;

    public event Action OnInputDevicesChanged;
    public Dictionary<string, bool> inputDevices;

    private Dictionary<string, (Action<InputAction.CallbackContext> OnPressed, Action<InputAction.CallbackContext> OnReleased)> _inputSystem_actionHandlers;
    private Dictionary<string, AnswerFromParticipant> _actionNameToSignalMap;


    private float _checkCedrusPortConnectionTimeInterval    = 0.1f; // sec


    public override void ManagedAwake()
    {
        _inputLogic = GetComponent<InputLogic>();
        _ui = GetComponent<UiHandler>();
        //_cedrus = GetComponent<CedrusHandler>();

        _inputActions = Resources.Load<InputActionAsset>("InputActions");    // name of file
        inputDevices = new();


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


    public override void ManagedStart()
    {

        //StartCoroutine(_cedrus.CheckConnection(_checkCedrusPortConnectionTimeInterval));

        // todo later: read flags from config
        foreach (var device in InputSystem.devices)
            inputDevices[device.displayName] = true;

        InputSystem.onDeviceChange += OnDeviceChange;

        CanUseUpdateMethod = true;
    }

    private void OnDeviceChange(UnityEngine.InputSystem.InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Added:
                inputDevices[device.displayName] = true;
                print($"Устройство добавлено: {device.displayName}");
                break;

            case InputDeviceChange.Removed:
                inputDevices.Remove(device.displayName);
                print($"Устройство удалено: {device.displayName}");
                break;
        }

        OnInputDevicesChanged?.Invoke();
    }

    //THE NESTING IN FOLLOWING FUNCTIONS MAY SEEM REDUNDANT, BUT LET IT BE JUST IN CASE


    // TODO: yes, it will be working by Ignoring
    private void GotSignalFromInputSystem(InputAction.CallbackContext context)
    {
        // todo: work on it. Maye just ignore if it's not in the list of active devices
        var devicesList = InputSystem.devices;
        var device = context.control.device;

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
}