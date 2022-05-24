using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampEntity : HassEntity
{
    public GameObject high;
    public MeshRenderer mr;
    public Color32 emColor;
    void Start()
    {
        mr.material = new Material(mr.material);
        mr.material.SetColor("_EmissionColor", Color.black);
    }
    public override void MouseOn()
    {
        high.SetActive(true);
    }
    public override void MouseExit()
    {
        high.SetActive(false);
    }

    protected override void TrunOn()
    {
        Color c = emColor;
        c.r *= 2;
        c.g *= 2;
        c.b *= 2;
        mr.material.SetColor("_EmissionColor", c);
        Debug.Log(emColor);
    }

    protected override void TrunOff()
    {
        mr.material.SetColor("_EmissionColor", Color.black);
    }

    public override void ReconstitutionMode(bool enter)
    {
        //base.ReconstitutionMode(enter);
        if (enter)
        {
            TrunOff();
        }
        else
        {
            if (open)
            {
                TrunOn();
            }
        }
    }
}
