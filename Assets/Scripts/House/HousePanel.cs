using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HousePanel : MonoBehaviour
{
    public float RoSpeed => ro.value;
    public static HousePanel Instance;
    public Slider tianguang, rain, snow, fog, lighting, ro, ranyuanjin;
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        tianguang.onValueChanged.AddListener(HouseWeather.Instance.SetTianGuang);
        rain.onValueChanged.AddListener(HouseWeather.Instance.SetRain);
        snow.onValueChanged.AddListener(HouseWeather.Instance.SetSnow);
        fog.onValueChanged.AddListener(HouseWeather.Instance.SetFog);
        lighting.onValueChanged.AddListener(HouseWeather.Instance.SetLighting);
        ranyuanjin.onValueChanged.AddListener((x) => CameraControllerForUnity.Instance.targetdis = x);
    }

}
