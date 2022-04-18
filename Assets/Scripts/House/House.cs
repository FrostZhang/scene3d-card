using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    Dictionary<string, Color> colors;

    void Awake()
    {
        colors = new Dictionary<string, Color>(10);
        colors.Add("red", Color.red);
        colors.Add("grey", Color.grey);
        colors.Add("gray", Color.gray);
        colors.Add("yellow", Color.yellow);
        colors.Add("magenta", Color.magenta);
        colors.Add("cyan", Color.cyan);
        colors.Add("black", Color.black);
        colors.Add("white", Color.white);
        colors.Add("blue", Color.blue);
        colors.Add("green", Color.green);
    }

    void Start()
    {
        JsonWriter w = new JsonWriter();
        w.WriteObjectStart();
        w.WritePropertyName("ceshi");
        w.Write(213);
        w.WriteObjectEnd();
        Debug.Log(w.ToString());
        CreatWall("2.5,0,5,1,0.125,90", null);
        CreatWall("0,2.5,5.25,0.5,0.25,0", null);
        CreatWall("-2.5,0,5,1,0.125,90", null);
        CreatWall("0,-2.5,5.25,0.5,0.25,0", null);
        CreatGroud("8,6", null);
        StartCoroutine(CreatWFloor("hongse,0,0.5,2,3"));
    }

    public void CreatWFloor()
    {
        Help.Instance.ABLoad("floor", "hesei");
    }

    IEnumerator CreatWFloor(string str)
    {
        var ss = str.Split(',');
        if (ss.Length != 5)
            yield break;
        float x, y, w, h;
        float va;
        if (float.TryParse(ss[1], out va))
            x = va;
        else yield break;
        if (float.TryParse(ss[2], out va))
            y = va;
        else yield break;
        if (float.TryParse(ss[3], out va))
            w = va * 0.1f;
        else yield break;
        if (float.TryParse(ss[4], out va))
            h = va * 0.1f;
        else yield break;

        yield return Help.Instance.ABLoad("floor", ss[0]);
        var ab = Help.Instance.GetBundle("floor", ss[0]);
        var tr = ab.LoadAsset<GameObject>(ss[0]);
        tr = Instantiate(tr);
        tr.transform.position = new Vector3(x, 0.01f, y);
        tr.transform.localScale = new Vector3(w, 1, h);
    }

    public void CreatGroud(string floor, string color)
    {
        var ss = floor.Split(',');
        float va;
        if (ss.Length == 2)
        {
            float x, y;
            if (float.TryParse(ss[0], out va))
                x = va;
            else return;
            if (float.TryParse(ss[1], out va))
                y = va;
            else return;
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.localScale = new Vector3(x * 0.1f, 1, y * 0.1f);
            Destroy(plane.GetComponent<Collider>());
            var r = plane.GetComponent<Renderer>();
            var ma = r.material = new Material(r.material);
            Color wcolor;
            if (TryColor(color, out wcolor))
            {
                ma.color = wcolor;
            }
        }
    }

    public void CreatWall(string wall, string color)
    {
        var ws = wall.Split(',');
        float va;
        if (ws.Length == 6)
        {
            float x, y, w, h, t, a;
            if (float.TryParse(ws[0], out va))
                x = va;
            else return;
            if (float.TryParse(ws[1], out va))
                y = va;
            else return;
            if (float.TryParse(ws[2], out va))
                w = va;
            else return;
            if (float.TryParse(ws[3], out va))
                h = va;
            else return;
            if (float.TryParse(ws[4], out va))
                t = va;
            else return;
            if (float.TryParse(ws[5], out va))
                a = va;
            else return;
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localPosition = new Vector3(x, h * 0.5f, y);
            cube.transform.localScale = new Vector3(w, h, t);
            cube.transform.localEulerAngles = new Vector3(0, a, 0);
            Destroy(cube.GetComponent<Collider>());
            var r = cube.GetComponent<Renderer>();
            var ma = r.material = new Material(r.material);
            Color wcolor;
            if (TryColor(color, out wcolor))
            {
                ma.color = wcolor;
            }
        }
    }

    protected bool TryColor(string cstr, out Color va)
    {
        if (string.IsNullOrWhiteSpace(cstr))
        {

        }
        else if (cstr.StartsWith("#"))
        {
            return ColorUtility.TryParseHtmlString(cstr, out va);
        }
        else if (colors.ContainsKey(cstr))
        {
            va = colors[cstr];
            return true;
        }
        else
        {
            return RGBAStr2Color(cstr, out va);
        }
        va = Color.magenta;
        return false;

    }

    bool RGBAStr2Color(string str, out Color color)
    {
        var css = str.Split(',');
        byte va;
        if (css.Length == 4)
        {
            byte r, g, b, a;
            if (byte.TryParse(css[0], out va))
                r = va;
            else goto end;
            if (byte.TryParse(css[1], out va))
                g = va;
            else goto end;
            if (byte.TryParse(css[2], out va))
                b = va;
            else goto end;
            if (byte.TryParse(css[3], out va))
                a = va;
            else goto end;
            color = new Color32(r, g, b, a);
            return true;
        }
    end:
        color = Color.magenta;
        return false;
    }
}

public class HouseConfigData
{
    public class Wall
    {
        public string chicun;
        public string color;
    }
}