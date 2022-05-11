using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmEntity : HassEntity
{
    public LineRenderer line;
    //public Transform yuan;
    Material yuanMa;
    public MeshRenderer editR;
    public Material YuanMa { get => yuanMa; }

    void Awake()
    {
        yuanMa = editR.material;
    }

    protected override void TrunOn()
    {
        //yuan.gameObject.SetActive(true);
        line.gameObject.SetActive(true);
    }

    protected override void TrunOff()
    {
        //yuan.gameObject.SetActive(false);
        line.gameObject.SetActive(false);
    }

    public override void ReconstitutionMode(bool enter)
    {
        if (enter)
        {
            editR.enabled = true;
            //yuan.gameObject.SetActive(true);
            line.gameObject.SetActive(true);
        }
        else
        {
            if (!open)
            {
                editR.enabled = false;
                //yuan.gameObject.SetActive(false);
                line.gameObject.SetActive(false);
            }
        }
    }
}
