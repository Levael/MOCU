using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralScript : MonoBehaviour
{
    void Start()
    {
        QualitySettings.vSyncCount = 0;                                     // Disable VSync
        Application.targetFrameRate = 100;                                  // Application fps (when VR is on -- automatically switchs to VR fps)
    }
}
