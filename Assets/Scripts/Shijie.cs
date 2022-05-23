using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class Shijie : MonoBehaviour
{
    public static Action<string> OnGetConfig;
    public static Action<string> OnHassFlush;
    public static string domain;
    void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLInput.captureAllKeyboardInput = false;
#endif
        var path = Application.streamingAssetsPath;
        var index = path.IndexOf('/', 8);
        if (index != -1)
        {
            domain = path.Remove(index);
            Debug.Log(domain);
        }
    }

    private void Start()
    {
        var res = WhichStand();
        if (res == 1)
        {
            QualitySettings.SetQualityLevel(0);
            var data = Camera.main.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.None;
            HouseWeather.Instance.volume.enabled = true;
        }
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
    [System.Runtime.InteropServices.DllImport("__Internal")]
    static extern void AsherLink3DConfig(string mes);
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern float HelloFloat();

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

    public static void Config3D(string mes)
    {
#if UNITY_EDITOR
        Debug.Log(mes);
#else
        AsherLink3DConfig(mes);
#endif
    }

    public static float WhichStand()
    {
#if UNITY_EDITOR
        return 0;
#else
        return HelloFloat();
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
}

public class HassMoreInfo
{
    public string entity_id;
}