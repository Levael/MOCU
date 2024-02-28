using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.XR;

public class InputHandler : MonoBehaviour
{
    public InputActionAsset inputActions;
    private InputLogic      _inputLogic;
    private Cedrus          _cedrus;
    private UiHandler       _uiHandler;
    

    private Dictionary<string, (Action<InputAction.CallbackContext> OnPressed, Action<InputAction.CallbackContext> OnReleased)> _inputSystem_actionHandlers;
    private Dictionary<string, SignalFromParticipant> _actionNameToSignalMap;

    //public DeviceConnectionStatus GamepadConnectionStatus;
    //public DeviceConnectionStatus XRConnectionStatus;

    public StateTracker GamepadConnectionStatus;
    public StateTracker XRConnectionStatus;

    private float _checkGamepadConnectionTimeInterval       = 0.1f; // sec
    private float _checkCedrusPortConnectionTimeInterval    = 0.1f; // sec
    private float _checkXRConnectionTimeInterval            = 0.1f; // sec




    void Awake()
    {
        GamepadConnectionStatus = new StateTracker(new[] { "isConnected" });
        XRConnectionStatus = new StateTracker(new[] { "isConnected" });

        _inputLogic     = GetComponent<InputLogic>();
        _cedrus         = GetComponent<Cedrus>();
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
            { "Left",   SignalFromParticipant.Left },
            { "Right",  SignalFromParticipant.Right },
            { "Up",     SignalFromParticipant.Up },
            { "Down",   SignalFromParticipant.Down },
            { "Center", SignalFromParticipant.Center }
        };

        // Cedrus ASCII codes are stored in "Cedrus class"
        //private Dictionary<SignalFromParticipant, Action<string>> _cedrusSignalHandlers; (from header)
        /*_cedrusCodes_answerSignals_Relations = new() {
            { "a", SignalFromParticipant.Up    },
            { "b", SignalFromParticipant.Left   },
            { "c", SignalFromParticipant.Center },
            { "d", SignalFromParticipant.Right  },
            { "e", SignalFromParticipant.Down }
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
        // Due to the fact that Unity does not see Cedrus as a HID device, I had to write a separate class for it with its own event handler
        _cedrus.gotData += GotSignalFromCedrus;
    }


    void Start()
    {
        _cedrus.TryConnect(doRequestPortName: true);
        StartCoroutine(_cedrus.CheckPortConnection(_checkCedrusPortConnectionTimeInterval));
        StartCoroutine(CheckGamepadConnection(_checkGamepadConnectionTimeInterval));
        StartCoroutine(CheckXRConnection(_checkXRConnectionTimeInterval));
    }

    void Update() { }




    //THE NESTING IN FOLLOWING FUNCTIONS MAY SEEM REDUNDANT, BUT LET IT BE JUST IN CASE

    private void GotSignalFromCedrus(SignalFromParticipant signalFromParticipant)
    {
        _inputLogic.GotAnswerFromCedrus(signalFromParticipant);
    }
    private void CudrusButtonWasReleased(InputAction.CallbackContext context)
    {
        // In ASCII mode Cedrus can't detect if button was released, it sends only "pressEvent"
    }

    private void GotSignalFromInputSystem(InputAction.CallbackContext context)
    {
        if (_actionNameToSignalMap.TryGetValue(context.action.name, out SignalFromParticipant signalFromParticipant))
            _inputLogic.GotPressSignalFromInputSystem(signalFromParticipant);

    }
    private void InputSystemButtonWasReleased(InputAction.CallbackContext context)
    {
        if (_actionNameToSignalMap.TryGetValue(context.action.name, out SignalFromParticipant signalFromParticipant))
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




    // CONNECTION CHECKERS  // todo: maybe combine all of them into one function

    private IEnumerator CheckGamepadConnection(float checkConnectionTimeInterval)
    {
        while (true)
        {
            try
            {
                if (Gamepad.current != null)
                    GamepadConnectionStatus.UpdateSubState("isConnected", true);
                else
                    GamepadConnectionStatus.UpdateSubState("isConnected", false);
            }
            catch
            {
                GamepadConnectionStatus.UpdateSubState("isConnected", false);
            }

            yield return new WaitForSeconds(checkConnectionTimeInterval);
        }
    }

    /// <summary>
    /// it works half-way -- it doesn't understand if you turn off the deviceю
    /// and also considers it disabled even if it is connected but has not yet been put on your head
    /// </summary>
    private IEnumerator CheckXRConnection(float checkConnectionTimeInterval)
    {
        while (true)
        {
            try
            {
                if (XRSettings.isDeviceActive)
                    XRConnectionStatus.UpdateSubState("isConnected", true);
                else
                    XRConnectionStatus.UpdateSubState("isConnected", false);
            }
            catch
            {
                XRConnectionStatus.UpdateSubState("isConnected", false);
            }

            yield return new WaitForSeconds(checkConnectionTimeInterval);
        }
    }
}
