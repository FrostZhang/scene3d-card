using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HousePanel : MonoBehaviour
{
    public float RoSpeed => ro.value;
    public static HousePanel Instance;
    public Slider tianguang, rain, snow, fog, lighting, ro, ranyuanjin;
    public Button changeCamera,edit;
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
        ranyuanjin.onValueChanged.AddListener((x) =>
        {
            CameraControllerForUnity.Instance.targetdis = x;
            CameraControllerForUnity.Instance.targetorthographicSize = x;
        });
        changeCamera.onClick.AddListener(() =>
        {
            if (Reconstitution.Instance.IsEdit)
                return;
            if (CameraControllerForUnity.Instance.mode == CameraControllerForUnity.Mode.third)
            {
                CameraControllerForUnity.Instance.transform.position = new Vector3(0, -1, 0);
                CameraControllerForUnity.Instance.MobaFollow_orthogonal(CameraControllerForUnity.Instance.transform, new Vector2(90, 0), 15, 5);
            }
            else if (CameraControllerForUnity.Instance.xAngle == 0)
            {
                CameraControllerForUnity.Instance.MobaFollow_orthogonal(CameraControllerForUnity.Instance.transform, new Vector2(90, 90), 15, 5);
            }
            else if (CameraControllerForUnity.Instance.xAngle == 90)
            {
                CameraControllerForUnity.Instance.MobaFollow_orthogonal(CameraControllerForUnity.Instance.transform, new Vector2(90, 180), 15, 5);
            }
            else if (CameraControllerForUnity.Instance.xAngle == 180)
            {
                CameraControllerForUnity.Instance.MobaFollow_orthogonal(CameraControllerForUnity.Instance.transform, new Vector2(90, 270), 15, 5);
            }
            else if (CameraControllerForUnity.Instance.xAngle == 270)
            {
                CameraControllerForUnity.Instance.Thirdfocus(new Vector3(0, -1, 0), new Vector2(45, 0), 12);
            }
        });

        edit.onClick.AddListener(Edit);
    }

    private void Edit()
    {
        Reconstitution.Instance.InEditToggle();
    }
}
