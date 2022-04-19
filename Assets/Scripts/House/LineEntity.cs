using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineEntity : HassEntity
{
    public LineRenderer line;
    public Color oncolor;
    public Color offcolor;
    void Start()
    {
        if (oncolor != Color.clear)
        {
            line.startColor = oncolor;
            line.endColor = oncolor;
        }
    }

    protected override void TrunOn()
    {

    }

    protected override void TrunOff()
    {

    }
}
