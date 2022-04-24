using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shijie : MonoBehaviour
{
    public static Action<string> OnGetConfig;
    public static Action<string> OnHassFlush;
    void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLInput.captureAllKeyboardInput = false;
#endif
    }

    public void HassConfig(string configStr)
    {
        OnGetConfig.Invoke(configStr);
    }

    public void HassMessage(string str)
    {
        OnHassFlush?.Invoke(str);
    }

    [System.Runtime.InteropServices.DllImport("__Internal")]
    static extern void AsherLink3DStart();
    [System.Runtime.InteropServices.DllImport("__Internal")]
    static extern void AsherLink3DClickMessage(string mes);
    [System.Runtime.InteropServices.DllImport("__Internal")]
    static extern void AsherLink3DLongClickMessage(string mes);
    [System.Runtime.InteropServices.DllImport("__Internal")]
    static extern void AsherLink3DWebLog(string mes);

    public static void Link3DStart()
    {
#if UNITY_EDITOR
        Debug.Log("Link3DStart");
#else
        AsherLink3DStart();
#endif
    }

    public static void ClickMessage(string mes)
    {
#if UNITY_EDITOR
        Debug.Log(mes);
#else
        AsherLink3DClickMessage(mes);
#endif
    }

    public static void LongClickMessage(string mes)
    {
#if UNITY_EDITOR
        Debug.Log(mes);
#else
        AsherLink3DLongClickMessage(mes);
#endif
    }

    public static void Link3DWebLog(string mes)
    {
#if UNITY_EDITOR
        Debug.Log(mes);
#else
        AsherLink3DWebLog(mes);
#endif
    }

}

public class HssConfig
{
    public string canting;
    public string keting;
    public string fuwo;
    public string zhuwo;
    public string xiaowoshi;
    public string cufang;
    public string guodao;
    public string weishenjian;
}

public class HassServerMessage
{
    public string head;
    public string cmd;
    public string entity_id;
    public string temp;
}

public class HassMoreInfo
{
    public string entity_id;
}