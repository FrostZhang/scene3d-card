using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SysPanel : MonoBehaviour
{
    public Button save;

    private void Start()
    {
        save.onClick.AddListener(Save);
    }

    private void Save()
    {
        var hs = House.Instance.CureetHouse;
        foreach (var item in hs)
        {
            switch (item.Value)
            {
                case HouseEntityType.wall:
                    break;
                case HouseEntityType.floor:
                    break;
                case HouseEntityType.door:
                    break;
                case HouseEntityType.stand:
                    break;
                case HouseEntityType.sky:
                    break;
                case HouseEntityType.arealight:
                    break;
                case HouseEntityType.appliances:
                    break;
                case HouseEntityType.flowLine:
                    break;
                case HouseEntityType.animal:
                    break;
                case HouseEntityType.weather:
                    break;
                default:
                    break;
            }
        }
    }
}
