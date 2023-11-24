using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class MicrophoneToSpeaker : MonoBehaviour
{
    //private AudioSource audioSource;
    //public GameObject gameObjectUi;

    void Start()
    {
        /*((TextElement)gameObjectUi.GetComponent<UiReferences>().GetElement("info-module-textbox")).text = $"{Microphone.devices[0]}";
        ((TextElement)gameObjectUi.GetComponent<UiReferences>().GetElement("info-module-textbox")).text = $"{Microphone.devices[1]}";*/

        /*var audioSource = GetComponent<AudioSource>();
        audioSource.clip = Microphone.Start(Microphone.devices[0], true, 10, 44100);
        audioSource.loop = true;
        while (!(Microphone.GetPosition(null) > 0)) { }
        audioSource.Play();*/

        //var audioSource = GetComponent<AudioSource>();
        //audioSource.Play();
    }
}









/*using UnityEngine;
using UnityEngine.InputSystem;

public class MicrophoneToSpeaker : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Этот метод связан с действием в новой системе ввода
    public void OnHoldI(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            // Начать передачу звука с микрофона
            StartMicrophone();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            // Остановить передачу звука с микрофона
            StopMicrophone();
        }
    }

    void StartMicrophone()
    {
        if (Microphone.devices.Length > 0)
        {
            audioSource.clip = Microphone.Start(null, true, 10, 44100);
            audioSource.loop = true;
            // Немного подождем, прежде чем начать воспроизведение записи
            while (!(Microphone.GetPosition(null) > 0)) { }
            audioSource.Play();
        }
    }

    void StopMicrophone()
    {
        Microphone.End(null);
        audioSource.Stop();
    }
}
*/