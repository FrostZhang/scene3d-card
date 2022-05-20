using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineEntity : HassEntity
{
    public LineRenderer line;
    public Color32 Oncolor { get => oncolor; }
    public Color32 Offcolor { get => offcolor; }
    private Color oncolor;
    private Color offcolor;

    //void Start()
    //{
    //    if (oncolor != Color.clear)
    //    {
    //        line.startColor = oncolor;
    //        line.endColor = oncolor;
    //    }
    //}

    public void SetOnc(Color oncolor)
    {
        this.oncolor = oncolor;
        if (open || string.IsNullOrEmpty(entity_id))
            line.startColor = line.endColor = oncolor;
    }

    public void SetOffc(Color offcolor)
    {
        this.offcolor = offcolor;
        if (!open && !string.IsNullOrEmpty(entity_id))
            line.startColor = line.endColor = offcolor;
    }

    protected override void TrunOn()
    {
        line.startColor = line.endColor = oncolor;
    }

    protected override void TrunOff()
    {
        line.startColor = line.endColor = offcolor;
    }
}
