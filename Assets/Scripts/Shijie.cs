using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shijie : MonoBehaviour
{
    public Light light;
    public Slider slider;
    public Image im;
    public Color32 cH = new Color32(255, 255, 218, 255);
    public Color32 cL = new Color32(0, 0, 0, 255);
    public Material sky;
    public CameraControllerForUnity cameraController;
    public Slider rainS, snowS, ro;
    public ParticleSystem rain, snow;

    public Transform areaLights;
    List<Area> areas;
    void Awake()
    {
        slider.onValueChanged.AddListener(ClickShijie);
        ClickShijie(0.5f);
        var rmain = rain.main;
        rmain.maxParticles = (int)rainS.value;
        var smain = snow.main;
        smain.maxParticles = (int)snowS.value;
        rainS.onValueChanged.AddListener((x) => rmain.maxParticles = (int)x);
        snowS.onValueChanged.AddListener((x) => smain.maxParticles = (int)x);
        areas = new List<Area>(areaLights.GetComponentsInChildren<Area>());
#if UNITY_WEBGL && !UNITY_EDITOR
        Shijie.UnityStart();
        WebGLInput.captureAllKeyboardInput = false;
#endif
    }

    public void InitFromHass(string str)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WebLog(str);
#endif
        var ss = str.Split(' ');
        var a= areas.Find((x) => x.entity_id == ss[0]);
        if (a!=null)
        {
            a.ChangeFromHass(ss[1]);
        }
    }

    private void ClickShijie(float v)
    {
        im.color = light.color = Color.Lerp(cL, cH, v);
        sky.SetFloat("_Exposure", v + 0.15f);
    }

    void Update()
    {
        cameraController.xAngle += Time.deltaTime * ro.value;
    }

    [System.Runtime.InteropServices.DllImport("__Internal")]
    public static extern void UnityStart();
    [System.Runtime.InteropServices.DllImport("__Internal")]
    public static extern void LightMessage(string mes);
    [System.Runtime.InteropServices.DllImport("__Internal")]
    public static extern void WebLog(string mes);
}

public class HassMessage
{
    public string head;
    public string cmd;
    public string entity_id;
    public string temp;
}