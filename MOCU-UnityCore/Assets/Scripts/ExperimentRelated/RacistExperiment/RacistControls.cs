//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.7.0
//     from Assets/Scripts/ExperimentRelated/RacistExperiment/RacistControls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace RacistExperiment
{
    public partial class @RacistControls: IInputActionCollection2, IDisposable
    {
        public InputActionAsset asset { get; }
        public @RacistControls()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""RacistControls"",
    ""maps"": [
        {
            ""name"": ""Responses"",
            ""id"": ""8ede8d92-34e4-45c9-a8f4-3b1a0e0f2b37"",
            ""actions"": [
                {
                    ""name"": ""Up"",
                    ""type"": ""Button"",
                    ""id"": ""6ab2ce81-7a66-43b2-9edb-b134c96f5dbf"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Down"",
                    ""type"": ""Button"",
                    ""id"": ""ef50c983-16e8-4737-a09b-8b16e7a76f1d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Start"",
                    ""type"": ""Button"",
                    ""id"": ""66051259-549c-4d4b-81ac-e89933b9c21f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""c776c0c4-491a-4bb9-9c7e-18f9465a969e"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Up"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5618548b-1b4c-4e77-8d28-bb214ad0b35f"",
                    ""path"": ""<Gamepad>/dpad/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Up"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7240cf8a-3cca-4452-b07a-0c0f80dd75ac"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Down"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e6925b74-cd8f-4648-a13a-8700e7ca991d"",
                    ""path"": ""<Gamepad>/dpad/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Down"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2caabeb8-7915-4e6c-ac85-a560db595031"",
                    ""path"": ""<Keyboard>/numpadEnter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Start"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""678e3038-9022-4d4c-a905-4fda3b8e0a27"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Start"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // Responses
            m_Responses = asset.FindActionMap("Responses", throwIfNotFound: true);
            m_Responses_Up = m_Responses.FindAction("Up", throwIfNotFound: true);
            m_Responses_Down = m_Responses.FindAction("Down", throwIfNotFound: true);
            m_Responses_Start = m_Responses.FindAction("Start", throwIfNotFound: true);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }

        public IEnumerable<InputBinding> bindings => asset.bindings;

        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
        {
            return asset.FindAction(actionNameOrId, throwIfNotFound);
        }

        public int FindBinding(InputBinding bindingMask, out InputAction action)
        {
            return asset.FindBinding(bindingMask, out action);
        }

        // Responses
        private readonly InputActionMap m_Responses;
        private List<IResponsesActions> m_ResponsesActionsCallbackInterfaces = new List<IResponsesActions>();
        private readonly InputAction m_Responses_Up;
        private readonly InputAction m_Responses_Down;
        private readonly InputAction m_Responses_Start;
        public struct ResponsesActions
        {
            private @RacistControls m_Wrapper;
            public ResponsesActions(@RacistControls wrapper) { m_Wrapper = wrapper; }
            public InputAction @Up => m_Wrapper.m_Responses_Up;
            public InputAction @Down => m_Wrapper.m_Responses_Down;
            public InputAction @Start => m_Wrapper.m_Responses_Start;
            public InputActionMap Get() { return m_Wrapper.m_Responses; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(ResponsesActions set) { return set.Get(); }
            public void AddCallbacks(IResponsesActions instance)
            {
                if (instance == null || m_Wrapper.m_ResponsesActionsCallbackInterfaces.Contains(instance)) return;
                m_Wrapper.m_ResponsesActionsCallbackInterfaces.Add(instance);
                @Up.started += instance.OnUp;
                @Up.performed += instance.OnUp;
                @Up.canceled += instance.OnUp;
                @Down.started += instance.OnDown;
                @Down.performed += instance.OnDown;
                @Down.canceled += instance.OnDown;
                @Start.started += instance.OnStart;
                @Start.performed += instance.OnStart;
                @Start.canceled += instance.OnStart;
            }

            private void UnregisterCallbacks(IResponsesActions instance)
            {
                @Up.started -= instance.OnUp;
                @Up.performed -= instance.OnUp;
                @Up.canceled -= instance.OnUp;
                @Down.started -= instance.OnDown;
                @Down.performed -= instance.OnDown;
                @Down.canceled -= instance.OnDown;
                @Start.started -= instance.OnStart;
                @Start.performed -= instance.OnStart;
                @Start.canceled -= instance.OnStart;
            }

            public void RemoveCallbacks(IResponsesActions instance)
            {
                if (m_Wrapper.m_ResponsesActionsCallbackInterfaces.Remove(instance))
                    UnregisterCallbacks(instance);
            }

            public void SetCallbacks(IResponsesActions instance)
            {
                foreach (var item in m_Wrapper.m_ResponsesActionsCallbackInterfaces)
                    UnregisterCallbacks(item);
                m_Wrapper.m_ResponsesActionsCallbackInterfaces.Clear();
                AddCallbacks(instance);
            }
        }
        public ResponsesActions @Responses => new ResponsesActions(this);
        public interface IResponsesActions
        {
            void OnUp(InputAction.CallbackContext context);
            void OnDown(InputAction.CallbackContext context);
            void OnStart(InputAction.CallbackContext context);
        }
    }
}
