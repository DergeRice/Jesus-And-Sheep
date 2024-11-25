using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public static class Vibration
{
    #if UNITY_ANDROID && !UNITY_EDITOR
    public static AndroidJavaClass AndroidPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    public static AndroidJavaObject AndroidcurrentActivity = AndroidPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    public static AndroidJavaObject AndroidVibrator = AndroidcurrentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
    #endif
    public static void Vibrate()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        AndroidVibrator.Call("vibrate");
        #endif
    }
 
    public static void Vibrate(long milliseconds)
    { 
        #if UNITY_ANDROID && !UNITY_EDITOR
        AndroidVibrator.Call("vibrate", milliseconds);
        #endif
    }
    public static void Vibrate(long[] pattern, int repeat)
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        AndroidVibrator.Call("vibrate", pattern, repeat);
        #endif
    }
 
    public static void Cancel()
    { 
        #if UNITY_ANDROID && !UNITY_EDITOR
        AndroidVibrator.Call("cancel");
        Handheld.Vibrate();
        #endif
    }

    
 
}