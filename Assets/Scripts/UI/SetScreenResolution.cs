using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetScreenResolution : MonoBehaviour
{
    void Start()
    {
        Screen.SetResolution(1920, 1080, FullScreenMode.ExclusiveFullScreen);
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }
}
