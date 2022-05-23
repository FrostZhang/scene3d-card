using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextEntity : HassEntity
{

    [SerializeField] MeshRenderer mr;
    public TextMesh textMesh;

    public string front, back;
    public string value = "?";
    void Start()
    {
        mr.enabled = false;
        FlushText();
        editC.enabled = false;
    }
    protected override void Destine(string state)
    {
        value = state;
        FlushText();
    }

    protected override void TrunOn()
    {
        value = "On";
        FlushText();
    }

    protected override void TrunOff()
    {
        value = "Off";
        FlushText();
    }

    public override void ReconstitutionMode(bool enter)
    {
        base.ReconstitutionMode(enter);
        if (enter)
        {
            mr.enabled = true;
        }
        else
        {
            mr.enabled = false;
        }
    }

    internal void FlushText()
    {
        textMesh.text = front + value + back;
    }
}
