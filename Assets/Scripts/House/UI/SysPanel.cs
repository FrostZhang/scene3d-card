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
        var hs = House.Instance.House2Json();
        Shijie.Config3D(hs);
    }
}
