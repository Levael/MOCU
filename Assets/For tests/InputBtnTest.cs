using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ButtonPressHandler : MonoBehaviour
{
    public InputActionAsset inputActions;
    public GameObject gameObjectUi;

    private InputAction testAction_A_btnPressed;
    private InputAction keyboardIntercomAction;


    private void Awake()
    {
        testAction_A_btnPressed = inputActions.FindActionMap("Test").FindAction("TestAction");
        keyboardIntercomAction = inputActions.FindActionMap("Keyboard").FindAction("Intercom");
    }

    private void OnEnable()
    {
        testAction_A_btnPressed.Enable();
        testAction_A_btnPressed.performed += OnButtonPressed;
        testAction_A_btnPressed.canceled += OnButtonReleased;

        keyboardIntercomAction.Enable();
        keyboardIntercomAction.performed += IntercomPressed;
        keyboardIntercomAction.canceled  += IntercomReleased;
    }

    private void OnDisable()
    {
        testAction_A_btnPressed.performed -= OnButtonPressed;
        testAction_A_btnPressed.canceled -= OnButtonReleased;
        testAction_A_btnPressed.Disable();

        keyboardIntercomAction.performed -= IntercomPressed;
        keyboardIntercomAction.canceled  -= IntercomReleased;
        keyboardIntercomAction.Disable();
    }

    private void OnButtonPressed(InputAction.CallbackContext context)
    {
        ((TextElement)gameObjectUi.GetComponent<UiReferences>().GetElement("info-module-textbox")).text = "Controller";
    }

    private void OnButtonReleased(InputAction.CallbackContext context)
    {
        ((TextElement)gameObjectUi.GetComponent<UiReferences>().GetElement("info-module-textbox")).text = "--------------";
    }


    private void IntercomPressed(InputAction.CallbackContext context)
    {
        ((TextElement)gameObjectUi.GetComponent<UiReferences>().GetElement("info-module-textbox")).text = "Keyboard";
    }

    private void IntercomReleased(InputAction.CallbackContext context)
    {
        ((TextElement)gameObjectUi.GetComponent<UiReferences>().GetElement("info-module-textbox")).text = "--------------";
    }
}
