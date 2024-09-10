using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;


public class GeneralScript : MonoBehaviour, IControllableInitiation
{
    public bool IsComponentReady {  get; private set; }

    public void ControllableAwake() { }
    public void ControllableStart()
    {
        QualitySettings.vSyncCount = 0;                             // Disable VSync
        Application.targetFrameRate = 60;                           // Application fps (when VR is on -- automatically switchs to VR fps)
        XRSettings.gameViewRenderMode = GameViewRenderMode.None;    // prevents rendering VR view on monitor (works only in Build version)

        IsComponentReady = true;
    }
}