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
            time.text = n.Hour + " : " + n.Minute;
        }
    }
}
