using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    Transform parent;
    void Awake()
    {
        parent = new GameObject().transform;
    }

    void Start()
    {
        mainCa = Camera.main;
        string wallColor = "195,216,235,255";
        CreatWall("3.7506,-0.2897,2.433167,1,0.125,0", wallColor);
        CreatWall("1.1117,2.926,2.645943,1,0.125,0", wallColor);
        CreatWall("-4.1035,-4.53,0.3899986,1,0.125,0", wallColor);
        CreatWall("4.1213,-4.5336,0.3512418,1,0.1322375,0", wallColor);
        CreatWall("-0.2699,-0.26,2.158078,1,0.125,0", wallColor);
        CreatWall("-4.1916,4.465,1.461849,1,0.125,0", wallColor);
        CreatWall("-4.4674,-3.097,0.8893961,1,0.125,0", wallColor);
        CreatWall("2.5509,0.958,0.8671593,1,0.125,0", wallColor);
        CreatWall("-0.9473,0.958,2.58769,1,0.125,0", wallColor);
        CreatWall("1.7963,0.958,0.7185282,0.5134,0.125,0", wallColor);
        CreatWall("1.4004,-3.88,1.523783,1,0.125,0", wallColor);
        CreatWall("4.5771,-3.88,0.8043213,1,0.125,0", wallColor);
        CreatWall("-1.7078,-3.115,0.7367308,1,0.125,0", wallColor);
        CreatWall("2.2753,-4.53,0.4198744,1,0.125,0", wallColor);
        CreatWall("0.5867,-4.535,0.2674765,1,0.1349125,0", wallColor);
        CreatWall("4.4384,2.926,1.012607,1,0.125,0", wallColor);
        CreatWall("-1.9781,2.926,0.5189419,1,0.125,0", wallColor);
        CreatWall("-1.4825,-4.53,0.8897885,1,0.125,0", wallColor);

        CreatWall("-4.8955,0.6771,7.67382,1,0.125,90", wallColor);
        CreatWall("0.25,1.9325,2.036761,1,0.125,90", wallColor);
        CreatWall("2.179,1.9244,2.052851,1,0.125,90", wallColor);
        CreatWall("4.91,-0.4659,6.907583,1,0.125,90", wallColor);
        CreatWall("1.676,-2.0393,3.705132,1,0.125,90", wallColor);
        CreatWall("-1.29,-2.3744,4.349825,1,0.125,90", wallColor);
        CreatWall("2.105,-4.2131,0.7695358,1,0.125,90", wallColor);
        CreatWall("0.674,-4.2085,0.7788795,1,0.125,90", wallColor);
        CreatWall("-4.23,-3.8118,1.506382,1,0.125,90", wallColor);
        CreatWall("-2.169,3.72,1.506382,1,0.125,90", wallColor);
        CreatWall("4.239,-4.2102,0.7753551,1,0.125,90", wallColor);
        CreatGroud("10,10", null);
        StartCoroutine(CreatDoor("door1", "1.568,-0.274,-1,-90,0", "switch.xxxx", null));
        StartCoroutine(CreatDoor("door1", "1.769,-0.274,1,90,0", "switch.xxxx", null));
        StartCoroutine(CreatDoor("door1", "2.848,0.008,1,0,-90", "switch.xxxx", null));
        StartCoroutine(CreatFlowLine("0,0,0,0,1,0,0,1,5", "yellow", "red", "switch.xxxx"));
        StartCoroutine(CreatStand("clock1", "0,-0.365,0.772,0,0,0,1,1,1", null));
        StartCoroutine(CreatStand("window1", "-0.3,-4.55,0,0,0,0,1,1,1", null));
        StartCoroutine(CreatStand("window1", "3.182,-4.55,0,0,0,0,1,1,1", null));
        StartCoroutine(CreatStand("window1", "3.182,2.93,0,0,180,0,1,1,1", null));
        StartCoroutine(CreatStand("window1", "-0.97,2.93,0,0,180,0,1,1,1", null));
        StartCoroutine(CreatStand("window2", "-2.923,-4.459,0,0,0,0,1,1,1", null));
        StartCoroutine(CreatStand("roundtable1", "-3.781,1.615,0,0,0,0,1,1,1", null));
        StartCoroutine(CreatStand("chair1", "-4.244,1.173,0,0,45,0,1,1,1", null));
        StartCoroutine(CreatStand("chair1", "-3.318,1.172,0,0,-45,0,1,1,1", null));
        StartCoroutine(CreatStand("chair1", "-4.237,2.067,0,0,135,0,1,1,1", null));
        StartCoroutine(CreatStand("chair1", "-3.34,2.07,0,0,-135,0,1,1,1", null));
        StartCoroutine(CreatStand("tree", "-1.66,-0.27,0,0,0,0,1,1,1", null));
        StartCoroutine(CreatStand("tvstand", "-1.64,-1.525,0,0,0,0,1,1,1", null));
        StartCoroutine(CreatStand("bed1", "-0.202,-2.24,0,0,90,0,1,1,1", null));
        StartCoroutine(CreatStand("bed1", "3.811,-2.24,0,0,-90,0,1,1,1", null));
        StartCoroutine(CreatStand("bed1", "3.2,2.12,0,0,90,0,0.855,1,0.95", null));
        StartCoroutine(CreatStand("shelving", "-0.3542,-0.644,0,0,-90,0,1.218,1,3.35", null));
        StartCoroutine(CreatStand("shelving", "3.773,-0.64,0,0,-90,0,1.218,1,3.98", null));
        StartCoroutine(CreatStand("shelving", "4.48,1.27,0,0,0,0,1.218,1,5.69", null));
        StartCoroutine(CreatStand("shelving", "1.78,1.319,0,0,0,0,1,0.61,1.158", null));
        StartCoroutine(CreatStand("shelving", "-1.24,2.5,0,0,-90,0,1.218,1,3.32", null));
        StartCoroutine(CreatStand("shelving", "-0.115,1.97,0,0,0,0,1.218,1,3.35", null));
        StartCoroutine(CreatStand("sofa2", "-4.225,-2.11,0,0,-90,0,1.3,1.3,1.3", null));
        StartCoroutine(CreatStand("sofa2", "-4.225,-1.25,0,0,-90,0,1.3,1.3,1.3", null));
        StartCoroutine(CreatStand("sofa2", "-4.225,-0.387,0,0,-90,0,1.3,1.3,1.3", null));

        StartCoroutine(CreatFloor("wood", "0.1962,-2.06,2.77,3.418,0.5,0.5"));
        StartCoroutine(CreatFloor("wood", "3.304,-2.02,3.04,3.5,0.5,0.5"));
        StartCoroutine(CreatFloor("wood", "3.195,-4.10,1.928,0.67,0.1,0.1"));
        StartCoroutine(CreatFloor("wood", "-0.3,-4.11,1.79,0.69,0.1,0.1"));
        StartCoroutine(CreatFloor("wood", "3.9,0.44,1.857,1.24,0.5,0.5"));
        StartCoroutine(CreatFloor("wood", "3.54,1.96,2.53,1.8,0.5,0.5"));
        StartCoroutine(CreatFloor("tile4", "-3.516,0.652,2.539,7.36,1,4")); 
        StartCoroutine(CreatFloor("tile4", "-2.752,-3.747,2.8,1.44,1.5,1"));
        StartCoroutine(CreatFloor("tile4", "0.2632,0.355,5,1.06,2,0.5"));
        StartCoroutine(CreatFloor("tile4", "-1.8,-1.6,0.88,2.86,0.4,1.5"));
        StartCoroutine(CreatFloor("tile4", "-1.02,1.94,2.47,1.84,1.1,1.1"));
        StartCoroutine(CreatFloor("tile4", "1.2,1.35,1.785,0.936,0.8,0.4"));

        StartCoroutine(CreatAreaLight("-3.55,2.08,3,3,3", "light.xxx", null));
        StartCoroutine(CreatAreaLight("-2.958,-1.49,3,3,5", "light.xxx", null));
        StartCoroutine(CreatAreaLight("0.41,-2.24,2,3,5", "light.xxx", null));
        StartCoroutine(CreatAreaLight("3.27,-2.3,2,3,4", "light.xxx", null));
        StartCoroutine(CreatAreaLight("3.55,1.85,2,2,4", "light.xxx", null));
        StartCoroutine(CreatAreaLight("-1,1.93,2,2,2", "light.xxx", null));
        StartCoroutine(CreatAreaLight("0.593,0.33,4,0.8,5", "light.xxx", "178,149,70,255"));
        StartCoroutine(CreatAreaLight("1.22,2.4,1.79,0.8,2", "light.xxx", null));


        StartCoroutine(CreatSky("space"));
    }

    IEnumerator CreatSky(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            yield break;
        yield return Help.Instance.ABLoad("sky", str);
        var ab = Help.Instance.GetBundle("sky", str);
        if (ab)
        {
            var ma = ab.LoadAsset<Material>(str);
            if (ma)
                RenderSettings.skybox = ma;
        }
    }

    /// <summary>x z y anglexyz scalexyz </summary>
    IEnumerator CreatStand(string cusname, string str, string color)
    {
        var ss = str.Split(',');
        if (ss.Length == 9)
        {
            float va, x, y, z, rx, ry, rz, sx, sy, sz;
            if (float.TryParse(ss[0], out va)) x = va; else yield break;
            if (float.TryParse(ss[1], out va)) y = va; else yield break;
            if (float.TryParse(ss[2], out va)) z = va; else yield break;
            if (float.TryParse(ss[3], out va)) rx = va; else yield break;
            if (float.TryParse(ss[4], out va)) ry = va; else yield break;
            if (float.TryParse(ss[5], out va)) rz = va; else yield break;
            if (float.TryParse(ss[6], out va)) sx = va; else yield break;
            if (float.TryParse(ss[7], out va)) sy = va; else yield break;
            if (float.TryParse(ss[8], out va)) sz = va; else yield break;

            yield return Help.Instance.ABLoad("stand", cusname);
            var ab = Help.Instance.GetBundle("stand", cusname);
            if (ab == null)
                yield break;
            var tr = ab.LoadAsset<GameObject>(cusname);
            tr = Instantiate(tr, parent);
            tr.transform.position = new Vector3(x, z, y);
            tr.transform.eulerAngles = new Vector3(rx, ry, rz);
            tr.transform.localScale = new Vector3(sx, sy, sz);
        }
    }

    IEnumerator CreatFlowLine(string pos, string con, string coff, string entity)
    {
        var vs = GetPoss(pos);
        if (vs == null)
            yield break;
        yield return Help.Instance.ABLoad("effect", "flowline");
        var ab = Help.Instance.GetBundle("effect", "flowline");
        var tr = ab.LoadAsset<GameObject>("flowline");
        var lineE = Instantiate(tr, parent).GetComponent<LineEntity>();
        lineE.line.positionCount = vs.Length;
        lineE.line.SetPositions(vs);
        Color c;
        if (Help.Instance.TryColor(con, out c))
            lineE.oncolor = c;
        if (Help.Instance.TryColor(coff, out c))
            lineE.offcolor = c;
        lineE.SetEntity(entity);
    }

    Vector3[] GetPoss(string str)
    {
        var ss = str.Split(',');
        if (ss.Length % 3 == 0)
        {
            Vector3[] vs = new Vector3[ss.Length / 3];
            var n = 0;
            float va, x, y, z;
            for (int i = 0; i < ss.Length; i += 3)
            {
                if (float.TryParse(ss[i], out va)) x = va; else return null;
                if (float.TryParse(ss[i + 1], out va)) y = va; else return null;
                if (float.TryParse(ss[i + 2], out va)) z = va; else return null;
                vs[n] = new Vector3(x, y, z);
            }
            return vs;
        }
        return null;
    }

    /// <summary>x z scaleX angleopen angleclose </summary>
    IEnumerator CreatDoor(string cusname, string str, string id, string color)
    {
        var ss = str.Split(',');
        if (ss.Length == 5)
        {
            float x, y, w, o, c;
            float va;
            if (float.TryParse(ss[0], out va)) x = va; else yield break;
            if (float.TryParse(ss[1], out va)) y = va; else yield break;
            if (float.TryParse(ss[2], out va)) w = va; else yield break;
            if (float.TryParse(ss[3], out va)) o = va; else yield break;
            if (float.TryParse(ss[4], out va)) c = va; else yield break;

            yield return Help.Instance.ABLoad("door", cusname);
            var ab = Help.Instance.GetBundle("door", cusname);
            if (ab == null)
                yield break;
            var tr = ab.LoadAsset<GameObject>(cusname);
            tr = Instantiate(tr, parent);
            var p = tr.transform.position;
            p.x = x;
            p.z = y;
            tr.transform.position = p;
            var s = tr.transform.localScale;
            s.x = w;
            tr.transform.localScale = s;
            var le = tr.GetComponent<DoorEntity>();
            le.SetEntity(id);
            le.angleopen = o;
            le.angleclose = c;
        }
    }

    /// <summary>x y w h li </summary>
    IEnumerator CreatAreaLight(string str, string id, string color)
    {
        var ss = str.Split(',');
        if (ss.Length == 5)
        {
            float x, y, w, h, li;
            float va;
            if (float.TryParse(ss[0], out va)) x = va; else yield break;
            if (float.TryParse(ss[1], out va)) y = va; else yield break;
            if (float.TryParse(ss[2], out va)) w = va; else yield break;
            if (float.TryParse(ss[3], out va)) h = va; else yield break;
            if (float.TryParse(ss[4], out va)) li = va; else yield break;

            yield return Help.Instance.ABLoad("light", "arealight");
            var ab = Help.Instance.GetBundle("light", "arealight");
            var tr = ab.LoadAsset<GameObject>("arealight");
            tr = Instantiate(tr, parent);
            tr.transform.position = new Vector3(x, 0.01f, y);
            tr.transform.localScale = new Vector3(w, 0.1f, h);
            var le = tr.GetComponent<LightEntity>();
            le.SetEntity(id);
            le.clight.intensity = li;
            Color c;
            if (Help.Instance.TryColor(color, out c))
            {
                le.clight.color = c;
            }
        }
        else
        {

        }
    }

    IEnumerator CreatFloor(string cusname, string str)
    {
        var ss = str.Split(',');
        if (ss.Length != 6) yield break;
        float va, x, y, w, h, tx, ty;
        if (float.TryParse(ss[0], out va)) x = va; else yield break;
        if (float.TryParse(ss[1], out va)) y = va; else yield break;
        if (float.TryParse(ss[2], out va)) w = va * 0.1f; else yield break;
        if (float.TryParse(ss[3], out va)) h = va * 0.1f; else yield break;
        if (float.TryParse(ss[4], out va)) tx = va; else yield break;
        if (float.TryParse(ss[5], out va)) ty = va; else yield break;

        yield return Help.Instance.ABLoad("floor", cusname);
        var ab = Help.Instance.GetBundle("floor", cusname);
        var tr = ab.LoadAsset<GameObject>(cusname);
        tr = Instantiate(tr, parent);
        tr.transform.position = new Vector3(x, 0.01f, y);
        tr.transform.localScale = new Vector3(w, 1, h);
        var ma = tr.GetComponent<MeshRenderer>().material;
        ma = new Material(ma);
        ma.SetTextureScale("_BaseMap", new Vector2(tx, ty));
        tr.GetComponent<MeshRenderer>().material = ma;
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
            plane.transform.SetParent(parent);
            plane.transform.localScale = new Vector3(x * 0.1f, 1, y * 0.1f);
            Destroy(plane.GetComponent<Collider>());
            var r = plane.GetComponent<Renderer>();
            var ma = r.material = new Material(r.material);
            Color wcolor;
            if (Help.Instance.TryColor(color, out wcolor))
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
            if (float.TryParse(ws[0], out va)) x = va; else return;
            if (float.TryParse(ws[1], out va)) y = va; else return;
            if (float.TryParse(ws[2], out va)) w = va; else return;
            if (float.TryParse(ws[3], out va)) h = va; else return;
            if (float.TryParse(ws[4], out va)) t = va; else return;
            if (float.TryParse(ws[5], out va)) a = va; else return;
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(parent);
            cube.transform.localPosition = new Vector3(x, h * 0.5f, y);
            cube.transform.localScale = new Vector3(w, h, t);
            cube.transform.localEulerAngles = new Vector3(0, a, 0);
            Destroy(cube.GetComponent<Collider>());
            var r = cube.GetComponent<Renderer>();
            var ma = r.material = new Material(r.material);
            Color wcolor;
            if (Help.Instance.TryColor(color, out wcolor))
            {
                ma.color = wcolor;
            }
        }
    }

    Camera mainCa;
    private RaycastHit hit;
    private float maxDistance = 20;
    Transform lasthit;
    HassEntity entity;
    Vector3 lastDownpos;
    float lastDownT;
    void Update()
    {
        if (Physics.Raycast(mainCa.ScreenPointToRay(Input.mousePosition), out hit, maxDistance))
        {
            if (hit.transform != lasthit)
            {
                lasthit = hit.transform;
                if (entity)
                    entity.MouseExit();
                entity = hit.transform.GetComponent<HassEntity>();
                if (entity)
                    entity.MouseOn();
            }
            else if (entity)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    lastDownpos = Input.mousePosition;
                    lastDownT = Time.time;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    if (Vector3.Magnitude(Input.mousePosition - lastDownpos) < 9)
                    {
                        if (Time.time - lastDownT > 0.5f)
                        {
                            entity.LongClick();
                        }
                        else
                        {
                            entity.Click();
                        }
                    }
                }
            }
        }
        else
        {
            lasthit = null;
            if (entity)
            {
                entity.MouseExit();
                entity = null;
            }
        }
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