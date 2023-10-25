using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MatchFov : MonoBehaviour
{
    public Camera vrCamera; // Перетащите вашу VR-камеру сюда в инспекторе
    private Camera uiCamera;

    void Start()
    {

        vrCamera = GameObject.Find("VrHelmetCamera").GetComponent<Camera>();
        uiCamera = this.GetComponent<Camera>();
        
    }

    private void Update()
    {
        uiCamera.fieldOfView = vrCamera.fieldOfView;
    }
}
