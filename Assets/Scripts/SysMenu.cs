using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SysMenu : MonoBehaviour
{
    public Slider tianguang, rain, snow, fog, lighting, ro, ranyuanjin;
    public TextMesh time;

    void Start()
    {
        tianguang.onValueChanged.AddListener(Weather.Instance.SetTianGuang);
        Weather.Instance.SetTianGuang(tianguang.value);
        rain.onValueChanged.AddListener(Weather.Instance.SetRain);
        Weather.Instance.SetRain(rain.value);
        snow.onValueChanged.AddListener(Weather.Instance.SetSnow);
        Weather.Instance.SetSnow(snow.value);
        fog.onValueChanged.AddListener(Weather.Instance.SetFog);
        Weather.Instance.SetFog(fog.value);
        lighting.onValueChanged.AddListener(Weather.Instance.SetLighting);
        Weather.Instance.SetLighting(lighting.value);
        ranyuanjin.onValueChanged.AddListener((x) => CameraControllerForUnity.Instance.targetdis = x);
    }

    int lastminute;
    void Update()
    {
        CameraControllerForUnity.Instance.xAngle += Time.deltaTime * ro.value;
        if (lastminute != DateTime.Now.Minute)
        {
            var n = DateTime.Now;
            lastminute = n.Minute;
            time.text = n.Hour + " : " + n.Minute;
        }
    }
}