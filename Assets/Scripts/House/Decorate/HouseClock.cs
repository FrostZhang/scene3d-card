using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseClock : MonoBehaviour
{
    public TextMesh time;
    int lastminute;
    void Update()
    {
        if (lastminute != DateTime.Now.Minute)
        {
            var n = DateTime.Now;
            lastminute = n.Minute;
            if (n.Minute < 10)
                time.text = n.Hour + " : 0" + n.Minute;
            else
                time.text = n.Hour + " : " + n.Minute;
        }
    }
}
