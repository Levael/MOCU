using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralScript : MonoBehaviour
{
    void Start()
    {
        QualitySettings.vSyncCount = 0;                                     // Disable VSync
        Application.targetFrameRate = 150;                                  // Application fps
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
