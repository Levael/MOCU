using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Assets.Scripts;


public class InputHandler : MonoBehaviour
{
    public InputActionAsset inputActions;
    private InputLogic      _inputLogic;


    private InputAction _keyboardIntercomOutputAction;
    private InputAction _controllerIntercomInputAction;

    private InputAction _controllerLeftAction;
    private InputAction _controllerRightAction;
    private InputAction _controllerUpAction;
    private InputAction _controllerDownAction;
    private InputAction _controllerCenterAction;

    private Cedrus _cedrus;



    private void Awake()
    {
        _keyboardIntercomOutputAction   = inputActions.FindActionMap("Intercom").FindAction("Keyboard_output");
        _controllerIntercomInputAction  = inputActions.FindActionMap("Intercom").FindAction("Gamepad_input");

        // controller here is not a gamepad but those 5 btns in general
        _controllerLeftAction           = inputActions.FindActionMap("Controller").FindAction("Left");
        _controllerRightAction          = inputActions.FindActionMap("Controller").FindAction("Right");
        _controllerUpAction             = inputActions.FindActionMap("Controller").FindAction("Up");
        _controllerDownAction           = inputActions.FindActionMap("Controller").FindAction("Down");
        _controllerCenterAction         = inputActions.FindActionMap("Controller").FindAction("Center");
    }

    private void OnEnable()     // OnDisable is not done because I don't want to. Maybe add later
    {
        _inputLogic = GetComponent<InputLogic>();
        _cedrus = GetComponent<Cedrus>();



        _keyboardIntercomOutputAction.Enable();
        _controllerIntercomInputAction.Enable();

        _controllerLeftAction.Enable();
        _controllerRightAction.Enable();
        _controllerUpAction.Enable();
        _controllerDownAction.Enable();
        _controllerCenterAction.Enable();

        // TODO: maybe change all of it to:
        // _controller.performed += _inputLogic.ControllerPressed;
        // _controller.canceled += _inputLogic.ControllerReleased;

        _keyboardIntercomOutputAction.performed   += _inputLogic.IntercomFromResearcherPressed;
        _keyboardIntercomOutputAction.canceled    += _inputLogic.IntercomFromResearcherReleased;
        
        _controllerIntercomInputAction.performed += _inputLogic.IntercomFromParticipantPressed;
        _controllerIntercomInputAction.canceled  += _inputLogic.IntercomFromParticipantReleased;
        
        _controllerLeftAction.performed     += _inputLogic.AnswerLeftPressed;
        _controllerLeftAction.canceled      += _inputLogic.AnswerLeftReleased;

        _controllerRightAction.performed    += _inputLogic.AnswerRightPressed;
        _controllerRightAction.canceled     += _inputLogic.AnswerRightReleased;

        _controllerUpAction.performed       += _inputLogic.AnswerUpPressed;
        _controllerUpAction.canceled        += _inputLogic.AnswerUpReleased;

        _controllerDownAction.performed     += _inputLogic.AnswerDownPressed;
        _controllerDownAction.canceled      += _inputLogic.AnswerDownReleased;

        _controllerCenterAction.performed   += _inputLogic.AnswerCenterPressed;
        _controllerCenterAction.canceled    += _inputLogic.AnswerCenterReleased;

        _cedrus.gotData += _inputLogic.GotAnswerFromCedrus;


        // here will be Cedrus EventListeners
        // ...

        // here will be Ui EventListeners
        // ...
    }

    private void Start()
    {
        //_inputLogic.TestMethod(_cedrus.answer);
        //StartCoroutine(CheckPortsCoroutine());
    }
}
