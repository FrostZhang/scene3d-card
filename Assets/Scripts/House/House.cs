using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{

    void Awake()
    {

    }

    void Start()
    {
        mainCa = Camera.main;
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
        StartCoroutine(CreatFloor("wood,0,0.5,2,3"));
        StartCoroutine(CreatAreaLight("light.xxxx,0,1.25,1.5,2,3"));
        StartCoroutine(CreatDoor("switch.xxxx,0,1.25,1,115,0"));
        StartCoroutine(CreatDoor("switch.xxxx,1,1.25,1,0,90"));
        StartCoroutine(CreatDoor("switch.xxxx,2,1.25,1,0,90"));
        StartCoroutine(CreatFlowLine("0,0,0,0,1,0,0,1,5", "yellow", "red", "switch.xxxx"));
        StartCoroutine(CreatAppliances("clock1,0,1,1.5,0"));
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

    IEnumerator CreatAppliances(string str)
    {
        var ss = str.Split(',');
        if (ss.Length == 5)
        {
            string name = ss[0];
            float va, x, y, z, r;
            if (float.TryParse(ss[1], out va)) x = va; else yield break;
            if (float.TryParse(ss[2], out va)) y = va; else yield break;
            if (float.TryParse(ss[3], out va)) z = va; else yield break;
            if (float.TryParse(ss[4], out va)) r = va; else yield break;

            yield return Help.Instance.ABLoad("appliances", name);
            var ab = Help.Instance.GetBundle("appliances", name);
            var tr = ab.LoadAsset<GameObject>(name);
            tr = Instantiate(tr);
            tr.transform.position = new Vector3(x, z, y);
            var e = tr.transform.localEulerAngles;
            e.y = r;
            tr.transform.localEulerAngles = e;
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
        var lineE = Instantiate(tr).GetComponent<LineEntity>();
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

    IEnumerator CreatDoor(string str)
    {
        var ss = str.Split(',');
        if (ss.Length == 6)
        {
            string id = ss[0];
            float x, y, w, o, c;
            float va;
            if (float.TryParse(ss[1], out va)) x = va; else yield break;
            if (float.TryParse(ss[2], out va)) y = va; else yield break;
            if (float.TryParse(ss[3], out va)) w = va; else yield break;
            if (float.TryParse(ss[4], out va)) o = va; else yield break;
            if (float.TryParse(ss[5], out va)) c = va; else yield break;

            yield return Help.Instance.ABLoad("door", "door1");
            var ab = Help.Instance.GetBundle("door", "door1");
            var tr = ab.LoadAsset<GameObject>("door1");
            tr = Instantiate(tr);
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

    IEnumerator CreatAreaLight(string str)
    {
        var ss = str.Split(',');
        if (ss.Length == 6)
        {
            string id = ss[0];
            float x, y, w, h, li;
            float va;
            if (float.TryParse(ss[1], out va)) x = va; else yield break;
            if (float.TryParse(ss[2], out va)) y = va; else yield break;
            if (float.TryParse(ss[3], out va)) w = va; else yield break;
            if (float.TryParse(ss[4], out va)) h = va; else yield break;
            if (float.TryParse(ss[5], out va)) li = va; else yield break;

            yield return Help.Instance.ABLoad("light", "arealight");
            var ab = Help.Instance.GetBundle("light", "arealight");
            var tr = ab.LoadAsset<GameObject>("arealight");
            tr = Instantiate(tr);
            tr.transform.position = new Vector3(x, 0.01f, y);
            var le = tr.GetComponent<LightEntity>();
            le.SetEntity(id);
            le.clight.intensity = li;
        }
        else
        {

        }
    }

    IEnumerator CreatFloor(string str)
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