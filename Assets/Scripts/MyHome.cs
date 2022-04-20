using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyHome : MonoBehaviour
{
    public Transform areaLights;
    List<Area> areas;

    void Start()
    {
        QualitySettings.SetQualityLevel(2);
        areas = new List<Area>(areaLights.GetComponentsInChildren<Area>());
        Shijie.OnGetConfig += OnGetConfig;
        Shijie.OnHassFlush += OnHassFlush;
    }

    private void OnHassFlush(string str)
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
                a.ChangeFromHass(ss[1]);
            }
        }
    }

    private void OnGetConfig(string configStr)
    {
        var hc = JsonUtility.FromJson<HssConfig>(configStr);
        var t = hc.GetType();
        Shijie.Link3DWebLog("3DªÒµ√≈‰÷√:" + configStr);
        foreach (var item in areas)
        {
            var a = t.GetField(item.name);
            if (a != null)
            {
                var value = a.GetValue(hc);
                if (value != null)
                {
                    item.SetHassEntity_id(a.GetValue(hc).ToString());
                    Shijie.Link3DWebLog(item.name + " SetID:" + a.GetValue(hc).ToString());
                }
                else
                {
                    item.SetHassEntity_id(string.Empty);
                }
            }
        }
    }
}
