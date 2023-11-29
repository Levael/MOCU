using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Assets.Scripts;


public class InputHandler : MonoBehaviour
{
    public InputActionAsset inputActions;
    public InputLogic       inputLogic;


    private InputAction _keyboardIntercomOutputAction;
    private InputAction _controllerIntercomInputAction;

    private InputAction _controllerLeftAction;
    private InputAction _controllerRightAction;
    private InputAction _controllerUpAction;
    private InputAction _controllerDownAction;
    private InputAction _controllerCenterAction;

    //private Cedrus _cedrus;



    private void Awake()
    {
        _keyboardIntercomOutputAction   = inputActions.FindActionMap("Intercom").FindAction("Keyboard_output");
        _controllerIntercomInputAction  = inputActions.FindActionMap("Intercom").FindAction("Gamepad_input");

        _controllerLeftAction           = inputActions.FindActionMap("Controller").FindAction("Left");
        _controllerRightAction          = inputActions.FindActionMap("Controller").FindAction("Right");
        _controllerUpAction             = inputActions.FindActionMap("Controller").FindAction("Up");
        _controllerDownAction           = inputActions.FindActionMap("Controller").FindAction("Down");
        _controllerCenterAction         = inputActions.FindActionMap("Controller").FindAction("Center");
    }

    private void OnEnable()     // OnDisable is not done because I don't want to. Maybe add later
    {
        _keyboardIntercomOutputAction.Enable();
        _controllerIntercomInputAction.Enable();

        _controllerLeftAction.Enable();
        _controllerRightAction.Enable();
        _controllerUpAction.Enable();
        _controllerDownAction.Enable();
        _controllerCenterAction.Enable();



        _keyboardIntercomOutputAction.performed   += inputLogic.IntercomFromResearcherPressed;
        _keyboardIntercomOutputAction.canceled    += inputLogic.IntercomFromResearcherReleased;
        
        _controllerIntercomInputAction.performed += inputLogic.IntercomFromParticipantPressed;
        _controllerIntercomInputAction.canceled  += inputLogic.IntercomFromParticipantReleased;
        
        _controllerLeftAction.performed     += inputLogic.AnswerLeftPressed;
        _controllerLeftAction.canceled      += inputLogic.AnswerLeftReleased;

        _controllerRightAction.performed    += inputLogic.AnswerRightPressed;
        _controllerRightAction.canceled     += inputLogic.AnswerRightReleased;

        _controllerUpAction.performed       += inputLogic.AnswerUpPressed;
        _controllerUpAction.canceled        += inputLogic.AnswerUpReleased;

        _controllerDownAction.performed     += inputLogic.AnswerDownPressed;
        _controllerDownAction.canceled      += inputLogic.AnswerDownReleased;

        _controllerCenterAction.performed   += inputLogic.AnswerCenterPressed;
        _controllerCenterAction.canceled    += inputLogic.AnswerCenterReleased;


        // here will be Cedrus EventListeners
        // ...

        // here will be Ui EventListeners
        // ...
    }

    private void Start()
    {
        //inputLogic.TestMethod(_cedrus.answer);
        //StartCoroutine(CheckPortsCoroutine());
    }



    private IEnumerator StartCheckPortsCoroutine()
    {
        while (true)
        {
            // code here
            yield return new WaitForSeconds(0.1f);  // 100ms delay
        }
    }
}
