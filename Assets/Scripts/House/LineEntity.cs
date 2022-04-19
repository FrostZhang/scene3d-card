using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineEntity : HassEntity
{
    Color oncolor;
    Color offcolor;
    LineRenderer line;
    void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    protected override void TrunOn()
    {

    }

    protected override void TrunOff()
    {

    }
}
