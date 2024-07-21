using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.XR;
using UnityEngine.InputSystem.Layouts;

public class InputHandler : MonoBehaviour
{
    public InputActionAsset inputActions;
    private InputLogic      _inputLogic;
    //private Cedrus_old          _cedrus;
    private UiHandler       _uiHandler;
    

    private Dictionary<string, (Action<InputAction.CallbackContext> OnPressed, Action<InputAction.CallbackContext> OnReleased)> _inputSystem_actionHandlers;
    private Dictionary<string, AnswerFromParticipant> _actionNameToSignalMap;


    public StateTracker GamepadConnectionStatus;
    public StateTracker XRConnectionStatus;

    private float _checkGamepadConnectionTimeInterval       = 0.1f; // sec
    private float _checkCedrusPortConnectionTimeInterval    = 0.1f; // sec
    private float _checkXRConnectionTimeInterval            = 0.1f; // sec

    //private UnityEngine.InputSystem.InputDevice virtualGamepad;


    void Awake()
    {
        GamepadConnectionStatus = new StateTracker(typeof(AnswerDevice_Statuses));
        XRConnectionStatus = new StateTracker(typeof(VrHeadset_Statuses));

        _inputLogic     = GetComponent<InputLogic>();
        //_cedrus         = GetComponent<Cedrus_old>();
        _uiHandler      = GetComponent<UiHandler>();
        

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

        // Cedrus_old ASCII codes are stored in "Cedrus_old class"
        //private Dictionary<AnswerFromParticipant, Action<string>> _cedrusSignalHandlers; (from header)
        /*_codes_ProtocolASCII = new() {
            { "a", AnswerFromParticipant.Up    },
            { "b", AnswerFromParticipant.Left   },
            { "c", AnswerFromParticipant.Center },
            { "d", AnswerFromParticipant.Right  },
            { "e", AnswerFromParticipant.Down }
        };*/

        // activates every action from InputSystem and, if it's in dict, adds its handler
        foreach (var actionMap in inputActions.actionMaps)
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


        // CEDRUS PART
        // Due to the fact that Unity does not see Cedrus_old as a HID device, I had to write a separate class for it with its own event handler
        //_cedrus.gotData += GotSignalFromCedrus;
    }


    void Start()
    {
        //_cedrus.TryConnect(doRequestPortName: true);
        //StartCoroutine(_cedrus.CheckPortConnection(_checkCedrusPortConnectionTimeInterval));
        StartCoroutine(CheckGamepadConnection(_checkGamepadConnectionTimeInterval));
        StartCoroutine(CheckXRConnection(_checkXRConnectionTimeInterval));

        //virtualGamepad = InputSystem.AddDevice<Gamepad>("Cedrus");

        var devices = InputSystem.devices;
        foreach (var device in devices)
        {
            Debug.Log($"Device: {device.displayName}, Type: {device.GetType().Name}, Name: {device.name}");
        }
    }

    void Update() { }

    void OnDestroy()
    {
        // Удаляем виртуальные устройства при уничтожении объекта
        /*if (virtualGamepad != null)
            InputSystem.RemoveDevice(virtualGamepad);*/
    }




    //THE NESTING IN FOLLOWING FUNCTIONS MAY SEEM REDUNDANT, BUT LET IT BE JUST IN CASE

    private void GotSignalFromCedrus(AnswerFromParticipant signalFromParticipant)
    {
        _inputLogic.GotAnswerFromCedrus(signalFromParticipant);
    }
    private void CudrusButtonWasReleased(InputAction.CallbackContext context)
    {
        // In ASCII mode Cedrus_old can't detect if button was released, it sends only "pressEvent"
    }

    private void GotSignalFromInputSystem(InputAction.CallbackContext context)
    {
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
