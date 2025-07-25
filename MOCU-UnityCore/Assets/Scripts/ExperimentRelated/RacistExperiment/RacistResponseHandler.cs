using UnityEngine;
using UnityEngine.InputSystem;


namespace RacistExperiment
{
    public class RacistResponseHandler : MonoBehaviour
    {
        private RacistControls _input;

        private void Awake()
        {
            _input = new();

            _input.Responses.Up.performed += _ => HandleUp();
            _input.Responses.Down.performed += _ => HandleDown();
        }

        private void OnEnable() => _input.Enable();
        private void OnDisable() => _input.Disable();

        private void HandleUp()
        {
            Debug.Log("Up arrow pressed");
        }

        private void HandleDown()
        {
            Debug.Log("Down arrow pressed");
        }
    }
}