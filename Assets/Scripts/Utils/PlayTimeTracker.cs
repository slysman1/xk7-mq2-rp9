#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

[InitializeOnLoad]
public static class PlayTimeTracker
{
//    private static DateTime playStartTime;

//    static PlayTimeTracker()
//    {
//        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
//    }

//    private static void OnPlayModeStateChanged(PlayModeStateChange state)
//    {
//        if (state == PlayModeStateChange.EnteredPlayMode)
//        {
//            playStartTime = DateTime.Now;
//            Debug.Log("? Play mode started at: " + playStartTime);
//        }
//        else if (state == PlayModeStateChange.ExitingPlayMode)
//        {
//            TimeSpan duration = DateTime.Now - playStartTime;
//            Debug.Log($"? Play mode stopped. Duration: {duration:mm\\:ss} (min:sec)");
//        }
//    }
}
#endif
