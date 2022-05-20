using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmEntity : HassEntity
{
    public LineRenderer line;
    public Transform yuan;
    Material yuanMa;
    public MeshRenderer editR;
    public Material YuanMa { get => yuanMa; }

    void Awake()
    {
        yuanMa = editR.material;
        TrunOff();
        editC = GetComponent<Collider>();
        editC.enabled = false;
    }

    protected override void TrunOn()
    {
        editR.enabled = true;
        line.gameObject.SetActive(true);
        HouseWeather.Instance.SetInDanger(true);
    }

    protected override void TrunOff()
    {
        editR.enabled = false;
        line.gameObject.SetActive(false);
        HouseWeather.Instance.SetInDanger(false);
    }

    public override void ReconstitutionMode(bool enter)
    {
        base.ReconstitutionMode(enter);
        HouseWeather.Instance.SetInDanger(false);
        if (enter)
        {
            editR.enabled = true;
            line.gameObject.SetActive(true);
        }
        else
        {
            if (!open)
            {
                editR.enabled = false;
                line.gameObject.SetActive(false);
            }
            else
            {
                HouseWeather.Instance.SetInDanger(true);
            }
        }
    }
}