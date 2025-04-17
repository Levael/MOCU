using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;


public class GeneralScript : ManagedMonoBehaviour
{
    public override void ManagedAwake() { }
    public override void ManagedStart()
    {
        QualitySettings.vSyncCount = 0;                             // Disable VSync
        Application.targetFrameRate = 60;                           // Application fps (when VR is on -- automatically switchs to VR fps (90))
        XRSettings.gameViewRenderMode = GameViewRenderMode.None;    // prevents rendering VR view on monitor (works only in Build version)

        IsComponentReady = true;
    }
}