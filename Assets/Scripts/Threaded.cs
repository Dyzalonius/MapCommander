using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public static class Threaded
{
    public static IEnumerator RunOnThread(Action toRun, Action callback = null)
    {
        bool done = false;

        // Run toRun, set done to true when finished
        new Thread(() => {
            toRun();
            done = true;
        }).Start();

        // Wait until done
        while (!done)
            yield return null;

        // Run callback
        if (callback != null)
            callback();
    }
}
