using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window : MonoBehaviour
{
    private const int SCREEN_WIDTH = 1024;
    private const int SCREEN_HEIGHT = 768;

    private void Awake()
    {
        if (Screen.width != SCREEN_WIDTH || Screen.height != SCREEN_HEIGHT)
        {
            Screen.SetResolution(SCREEN_WIDTH, SCREEN_HEIGHT, false);
            PlayerPrefs.SetInt("Screenmanager Resolution Width", SCREEN_WIDTH);
            PlayerPrefs.SetInt("Screenmanager Resolution Height", SCREEN_HEIGHT);
        }
    }
}
