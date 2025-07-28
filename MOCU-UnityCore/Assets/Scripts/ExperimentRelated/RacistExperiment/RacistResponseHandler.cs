using System;
using UnityEngine;
using UnityEngine.InputSystem;


namespace RacistExperiment
{
    public class RacistResponseHandler : MonoBehaviour
    {
        public event Action GotAnswer_Up;
        public event Action GotAnswer_Down;
        public event Action GotSignal_Start;

        private RacistControls _input;

        private void Awake()
        {
            _input = new();

            _input.Responses.Up.performed += _ => HandleUp();
            _input.Responses.Down.performed += _ => HandleDown();
            _input.Responses.Start.performed += _ => HandleStart();
        }

        private void OnEnable() => _input.Enable();
        private void OnDisable() => _input.Disable();

        private void HandleUp()
        {
            Debug.Log("Up arrow pressed");
            GotAnswer_Up?.Invoke();
        }

        private void HandleDown()
        {
            Debug.Log("Down arrow pressed");
            GotAnswer_Down?.Invoke();
        }

        private void HandleStart()
        {
            Debug.Log("Start button pressed");
            GotSignal_Start?.Invoke();
        }
    }
}