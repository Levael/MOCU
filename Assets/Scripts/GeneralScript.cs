using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralScript : MonoBehaviour
{
    void Start()
    {
        QualitySettings.vSyncCount = 0;                                     // Disable VSync
        Application.targetFrameRate = 150;                                  // Application fps

        Display.displays[0].Activate();                                     // activation of main display (without it window isn't fullscreen)
        if (Display.displays.Length > 1) Display.displays[1].Activate();    // activation of second display (if needed)

    }




    /*void Update()
    {
        
    }*/


    void ActivateDisplays()
    {
        for (int i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }
    }
}
