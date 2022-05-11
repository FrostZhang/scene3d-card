using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diqiu : MonoBehaviour
{
    public float radio  = 3000;

    private Vector3 CalculateJW2Pos(Vector2 jw)
    {
        float j = jw.x * Mathf.Deg2Rad;
        float w = (90 - jw.y) * Mathf.Deg2Rad;
        float x = radio * Mathf.Sin(w) * Mathf.Cos(j);
        float y = radio * Mathf.Sin(w) * Mathf.Sin(j);
        float z = radio * Mathf.Cos(w);

        return new Vector3(x, z, y);
    }

    string CalculatePos2JW(Vector3 Tr, Vector3 worldpos, out Vector2 jw)
    {
        jw = new Vector2();
        var myRadius = Vector3.Distance(Tr, worldpos);

        var myLatitude = Mathf.Asin(worldpos.y / myRadius) * Mathf.Rad2Deg;//求纬度，并转成弧度
                                                                           //注：这里我是以 x,z 为水平面 ，即 y = 0 的轨道为赤道，在Unity中可以很直观地看出
        var myLatDegree = (int)Mathf.Abs(myLatitude);//纬度均为正数，取绝对值,然后判断南北纬
        var myLatMinute = (int)(Mathf.Abs(myLatitude) * 60) % 60;
        var myLatSecond = (Mathf.Abs(myLatitude) * 3600) % 60;
        var LatDire = myLatitude < 0 ? 'S' : 'N';

        var myLonitude = Mathf.Atan2(worldpos.z, worldpos.x) * Mathf.Rad2Deg;//求经度
        var myLonDegree = (int)Mathf.Abs(myLonitude);
        var myLonMinute = (int)(Mathf.Abs(myLonitude) * 60) % 60;
        var myLonSecond = (Mathf.Abs(myLonitude) * 3600) % 60;
        var LonDire = myLonitude < 0 ? 'W' : 'E';
        jw.x = myLonitude;
        jw.y = myLatitude;
        return string.Format("{0}°{1}'{2:F0}\"{3}  {4}°{5}'{6:F0}\"{7}", myLatDegree, myLatMinute, myLatSecond, LatDire,
            myLonDegree, myLonMinute, myLonSecond, LonDire);
    }
}
