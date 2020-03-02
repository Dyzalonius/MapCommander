﻿using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public static class Threaded
{
    public static IEnumerator RunOnThread(Action toRun, Action callback)
    {
        bool done = false;
        Debug.Log(Time.realtimeSinceStartup + " - START");

        // Run toRun, set done to true when finished
        new Thread(() => {
            toRun();
            done = true;
        }).Start();

        // Wait until done
        while (!done)
            yield return null;

        Debug.Log(Time.realtimeSinceStartup + " - MIDDLE");

        // Run callback
        callback();

        Debug.Log(Time.realtimeSinceStartup + " - END");
    }
}
