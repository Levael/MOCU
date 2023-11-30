using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class InputHandler : MonoBehaviour
{
    public InputActionAsset inputActions;
    private InputLogic      _inputLogic;
    private Cedrus          _cedrus;
    private UiHandler       _uiHandler;

    /// Dictionary stores ActionName (from InputActionAsset) and tuple with two handlers: on "press" event and on "release" event
    private Dictionary<string, (Action<InputAction.CallbackContext> OnPressed, Action<InputAction.CallbackContext> OnReleased)> _inputSystem_actionHandlers;
    private Dictionary<string, SignalFromParticipant> _actionNameToSignalMap;
    



    private void Awake()
    {
        _inputLogic = GetComponent<InputLogic>();
        _cedrus     = GetComponent<Cedrus>();
        _uiHandler  = GetComponent<UiHandler>();

        // INPUT SYSTEM PART (gamepad, keyboard and other devices Unity support)
        // Dictionary keys -- as specified in "InputActionAsset" (action.name)
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

    private void Start()
    {
    }



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
}
