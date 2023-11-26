using UnityEngine;
using UnityEngine.InputSystem;


public class InputHandler : MonoBehaviour
{
    public InputActionAsset inputActions;
    public ButtonsLogic     buttonsLogic;


    private InputAction _keyboardIntercomAction;

    private InputAction _controllerLeftAction;
    private InputAction _controllerRightAction;
    private InputAction _controllerUpAction;
    private InputAction _controllerDownAction;
    private InputAction _controllerCenterAction;
    private InputAction _controllerIntercomAction;


    private void Awake()
    {
        _keyboardIntercomAction     = inputActions.FindActionMap("Keyboard").FindAction("Intercom");

        _controllerLeftAction       = inputActions.FindActionMap("Controller").FindAction("Left");
        _controllerRightAction      = inputActions.FindActionMap("Controller").FindAction("Right");
        _controllerUpAction         = inputActions.FindActionMap("Controller").FindAction("Up");
        _controllerDownAction       = inputActions.FindActionMap("Controller").FindAction("Down");
        _controllerCenterAction     = inputActions.FindActionMap("Controller").FindAction("Center");
        _controllerIntercomAction   = inputActions.FindActionMap("Controller").FindAction("Intercom");
    }

    private void OnEnable()
    {
        _keyboardIntercomAction.Enable();

        _controllerIntercomAction.Enable();
        _controllerLeftAction.Enable();
        _controllerRightAction.Enable();
        _controllerUpAction.Enable();
        _controllerDownAction.Enable();
        _controllerCenterAction.Enable();
        _controllerIntercomAction.Enable();

        // MAIN PART
        _keyboardIntercomAction.performed   += buttonsLogic.IntercomFromResearcherPressed;
        _keyboardIntercomAction.canceled    += buttonsLogic.IntercomFromResearcherReleased;
        
        _controllerIntercomAction.performed += buttonsLogic.IntercomFromParticipantPressed;
        _controllerIntercomAction.canceled  += buttonsLogic.IntercomFromParticipantReleased;
        
        _controllerLeftAction.performed     += buttonsLogic.AnswerLeftPressed;
        _controllerLeftAction.canceled      += buttonsLogic.AnswerLeftReleased;

        _controllerRightAction.performed    += buttonsLogic.AnswerRightPressed;
        _controllerRightAction.canceled     += buttonsLogic.AnswerRightReleased;

        _controllerUpAction.performed       += buttonsLogic.AnswerUpPressed;
        _controllerUpAction.canceled        += buttonsLogic.AnswerUpReleased;

        _controllerDownAction.performed     += buttonsLogic.AnswerDownPressed;
        _controllerDownAction.canceled      += buttonsLogic.AnswerDownReleased;

        _controllerCenterAction.performed   += buttonsLogic.AnswerCenterPressed;
        _controllerCenterAction.canceled    += buttonsLogic.AnswerCenterReleased;


        // here will be Cedrus EventListeners
        // ...
    }

    private void OnDisable()
    {
        _keyboardIntercomAction.Disable();
        _controllerIntercomAction.Disable();
        _controllerLeftAction.Disable();
        _controllerRightAction.Disable();
        _controllerUpAction.Disable();
        _controllerDownAction.Disable();
        _controllerCenterAction.Disable();
        _controllerIntercomAction.Disable();


        _keyboardIntercomAction.performed   -= buttonsLogic.IntercomFromResearcherPressed;
        _keyboardIntercomAction.canceled    -= buttonsLogic.IntercomFromResearcherReleased;

        _controllerIntercomAction.performed -= buttonsLogic.IntercomFromParticipantPressed;
        _controllerIntercomAction.canceled  -= buttonsLogic.IntercomFromParticipantReleased;

        _controllerLeftAction.performed     -= buttonsLogic.AnswerLeftPressed;
        _controllerLeftAction.canceled      -= buttonsLogic.AnswerLeftReleased;

        _controllerRightAction.performed    -= buttonsLogic.AnswerRightPressed;
        _controllerRightAction.canceled     -= buttonsLogic.AnswerRightReleased;

        _controllerUpAction.performed       -= buttonsLogic.AnswerUpPressed;
        _controllerUpAction.canceled        -= buttonsLogic.AnswerUpReleased;

        _controllerDownAction.performed     -= buttonsLogic.AnswerDownPressed;
        _controllerDownAction.canceled      -= buttonsLogic.AnswerDownReleased;

        _controllerCenterAction.performed   -= buttonsLogic.AnswerCenterPressed;
        _controllerCenterAction.canceled    -= buttonsLogic.AnswerCenterReleased;


        // here will be Cedrus EventListeners
        // ...
    }
}
