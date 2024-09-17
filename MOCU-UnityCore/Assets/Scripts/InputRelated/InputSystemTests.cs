using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;


public class InputSystemTests : MonoBehaviour, IControllableInitiation
{
    public bool IsComponentReady { get; private set; }
    public event Action<InputAction.CallbackContext> OnIntercomAction;
    public event Action<InputAction.CallbackContext> OnDPadAction;
    public event Action<InputAction.CallbackContext> OnJoystickAction;

    private Dictionary<
        ActionMapName,
        (Type actions, Action<InputAction.CallbackContext> personalHandler)
    > ReadableMapping;

    private Dictionary<
        (string mapName, string actionName),
        (ActionMapName mapName, Enum actionName, Action<InputAction.CallbackContext> personalHandler)
    > ActualMapping;


    public void ControllableAwake()
    {
        ReadableMapping = new()
        {
            { ActionMapName.Intercom,  ( typeof(IntercomAction), OnIntercomAction ) },
            { ActionMapName.DPad,      ( typeof(DPadAction),     OnDPadAction ) },
            { ActionMapName.Joystick,  ( typeof(JoystickAction), OnJoystickAction ) },

            // Add new pair if needed
        };

        ActualMapping = new();

        foreach (var entry in ReadableMapping)
        {
            var actionMapName = entry.Key;
            var (actionEnumType, personalHandler) = entry.Value;
            var actionEnumValues = Enum.GetValues(actionEnumType);

            InputActionMap actionMap = new InputActionMap(actionMapName.ToString());

            foreach (Enum action in actionEnumValues)
            {
                var inputAction = actionMap.AddAction(action.ToString());

                inputAction.started += context => CommonHandler(context, personalHandler);
                inputAction.canceled += context => CommonHandler(context, personalHandler);

                ActualMapping.Add(
                    (actionMap.name, inputAction.name),
                    (actionMapName, action, personalHandler)
                );
            }

            actionMap.Enable();
        }

        IsComponentReady = true;
    }

    public void ControllableStart() { }


    private void CommonHandler(InputAction.CallbackContext context, Action<InputAction.CallbackContext> personalHandler) {
        if (true)   // todo: change later (if actionMap is supported and active)
            personalHandler?.Invoke(context);
    }
}