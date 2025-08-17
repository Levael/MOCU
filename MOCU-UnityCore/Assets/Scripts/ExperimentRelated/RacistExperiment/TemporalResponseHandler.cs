using System;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Temporal
{
    public class TemporalResponseHandler : ManagedMonoBehaviour
    {
        public event Action GotAnswer_Up;
        public event Action GotAnswer_Down;
        public event Action GotAnswer_Left;
        public event Action GotAnswer_Right;
        public event Action GotSignal_Start;

        private TemporalControls _input;

        public override void ManagedAwake()
        {
            _input = new();

            _input.Responses.Up.performed += _ => HandleUp();
            _input.Responses.Down.performed += _ => HandleDown();
            _input.Responses.Left.performed += _ => HandleLeft();
            _input.Responses.Right.performed += _ => HandleRight();
            _input.Responses.Start.performed += _ => HandleStart();
        }

        public override void ManagedOnEnable() => _input.Enable();
        public override void ManagedOnDisable() => _input.Disable();

        // .................................

        private void HandleUp()
        {
            //Debug.Log("Up arrow pressed");
            GotAnswer_Up?.Invoke();
        }

        private void HandleDown()
        {
            //Debug.Log("Down arrow pressed");
            GotAnswer_Down?.Invoke();
        }

        private void HandleLeft()
        {
            //Debug.Log("Left arrow pressed");
            GotAnswer_Left?.Invoke();
        }

        private void HandleRight()
        {
            //Debug.Log("Right arrow pressed");
            GotAnswer_Right?.Invoke();
        }

        private void HandleStart()
        {
            //Debug.Log("Start button pressed");
            GotSignal_Start?.Invoke();
        }
    }
}