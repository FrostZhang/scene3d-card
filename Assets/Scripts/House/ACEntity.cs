using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACEntity : HassEntity
{
    [SerializeField] GameObject eff;
    [SerializeField] GameObject high;
    public override void MouseOn()
    {
        high.SetActive(true);
    }
    public override void MouseExit()
    {
        high.SetActive(false);
    }
    void Awake()
    {
        eff.SetActive(false);
    }
    protected override void TrunOn()
    {
        eff.SetActive(true);
    }

    protected override void TrunOff()
    {
        eff.SetActive(false);
    }
    public override void ReconstitutionMode(bool enter)
    {
        if (enter)
        {
            eff.SetActive(false);
        }
        else
        {
            if (open)
            {
                eff.SetActive(true);
            }
        }
    }
}
