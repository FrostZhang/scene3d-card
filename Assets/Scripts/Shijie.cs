using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shijie : MonoBehaviour
{
    public Transform areaLights;
    List<Area> areas;
    string lastWeather;
    void Awake()
    {
        areas = new List<Area>(areaLights.GetComponentsInChildren<Area>());
        QualitySettings.SetQualityLevel(2);
#if UNITY_WEBGL && !UNITY_EDITOR
        Shijie.AsherLink3DStart();
        WebGLInput.captureAllKeyboardInput = false;
#endif
    }

    public void HassConfig(string configStr)
    {
        var hc = JsonUtility.FromJson<HssConfig>(configStr);
        var t = hc.GetType();
        AsherLink3DWebLog("3D»ñµÃÅäÖÃ:" + configStr);
        foreach (var item in areas)
        {
            var a = t.GetField(item.name);
            if (a != null)
            {
                var value = a.GetValue(hc);
                if (value != null)
                {
                    item.SetHassEntity_id(a.GetValue(hc).ToString());
                    AsherLink3DWebLog(item.name + " SetID:" + a.GetValue(hc).ToString());
                }
                else
                {
                    item.SetHassEntity_id(string.Empty);
                }
            }
        }
    }

    public void HassMessage(string str)
    {
        var ss = str.Split(' ');
        if (ss[0] == "sun.sun")
        {
            float value;
            if (float.TryParse(ss[1], out value))
            {
                if (value > 0)
                {
                    var v2 = Mathf.Max(0.1f, Mathf.Sin(value * Mathf.Deg2Rad));
                    Weather.Instance.SetTianGuang(v2);
                }
                else
                    Weather.Instance.SetTianGuang(0.1f);
            }
        }
        else if (ss[0] == "weather.tian_qi")
        {
            Weather.Instance.SetWeather(ss[1]);
        }
        else
        {
            var a = areas.Find((x) => x.entity_id == ss[0]);
            if (a != null)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
        //WebLog(ss[0] +" " +ss[1]);
#endif
                a.ChangeFromHass(ss[1]);
            }
        }
    }

    [System.Runtime.InteropServices.DllImport("__Internal")]
    public static extern void AsherLink3DStart();
    [System.Runtime.InteropServices.DllImport("__Internal")]
    public static extern void AsherLink3DClickMessage(string mes);
    [System.Runtime.InteropServices.DllImport("__Internal")]
    public static extern void AsherLink3DLongClickMessage(string mes);
    [System.Runtime.InteropServices.DllImport("__Internal")]
    public static extern void AsherLink3DWebLog(string mes);
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