using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public enum HouseEntityType
{
    wall, floor, door, stand, sky, arealight, appliances, flowLine, animal, weather, eye
}
public class House : MonoBehaviour
{
    public static House Instance;
    public Transform parent { get; set; }
    public Dictionary<Transform, HouseEntityType> CureetHouse { get => cureetHouse; }
    public List<string> HomeassistantEntities { get => homeassistantEntities; }

    List<HassEntity> lis;
    Dictionary<Transform, HouseEntityType> cureetHouse;
    List<string> homeassistantEntities;
    void Awake()
    {
        Instance = this;
        parent = new GameObject().transform;
        mainCa = Camera.main;
        QualitySettings.SetQualityLevel(2);
        lis = new List<HassEntity>(100);
        homeassistantEntities = new List<string>(100);
        cureetHouse = new Dictionary<Transform, HouseEntityType>(200);
    }

    void Start()
    {
        Shijie.OnGetConfig += OnGetConfig;
        Shijie.OnHassFlush += OnHassFlush;
#if UNITY_WEBGL && !UNITY_EDITOR
        Shijie.Link3DStart();
        WebGLInput.captureAllKeyboardInput = false;
#endif

#if UNITY_EDITOR
        Test();
#else
    ReadCustomConfig();
#endif
    }

    private async void ReadCustomConfig()
    {
        var te = await Help.Instance.TextRequest(false, Shijie.domain + "/3dscene_local/Customdata/houseconnfig.json", false);
        if (te != null)
        {
            if (te.StartsWith(""))
                te = te.TrimStart('\"').TrimEnd('\"');
            te = te.Replace("None", "null");
            OnGetConfig(te);
        }
    }

    private async void Test()
    {
        var te = await Help.Instance.TextRequest(true, "Customdata/houseconnfig.json");
        if (te != null)
        {
            if (te.StartsWith(""))
                te = te.TrimStart('\"').TrimEnd('\"');
            te = te.Replace("None", "null");
            OnGetConfig(te);
            return;
        }
        TestRoom();
        await TestJson();
    }

    private async Task TestJson()
    {
        await new WaitForSeconds(2);
        var js = House2Json();
        Debug.Log(js);
        await new WaitForSeconds(2);
        OnGetConfig(js);
    }

    private void OnHassFlush(string message)
    {
        if (Reconstitution.Instance.IsEdit)
        {
            var hid = message.Split(' ')[0];
            if (!homeassistantEntities.Contains(hid))
            {
                homeassistantEntities.Add(hid);
            }
            return;
        }
        var ss = message.Split(' ');
        var id = ss[0];
        var va = ss[1];
        if (id == "sun.sun")
        {
            AnsSun(va);
        }
        else
        {
            foreach (var item in lis)
            {
                if (item.Entity_id == id)
                    item.StateMeassge(va);
            }
        }
    }

    internal void Delete(Transform lasthit)
    {
        if (CureetHouse.ContainsKey(lasthit))
        {
            var e = lasthit.GetComponentInChildren<HassEntity>(true);
            if (e) lis.Remove(e);
            CureetHouse.Remove(lasthit);
            Destroy(lasthit.gameObject);
        }
    }

    //float lastFlushTime;
    private void OnGetConfig(string config)
    {
        if (Reconstitution.Instance.IsEdit)
            return;
        var jd = JsonMapper.ToObject(config);
        if (jd == null)
            return;
        Destroy(parent.gameObject);
        lis.Clear();
        cureetHouse.Clear();
        lis.Add(HouseWeather.Instance);//×¢²áÌìÆø
        parent = new GameObject().transform;
        if (jd.IsObject)
        {
            var dic = jd as IDictionary;
            if (dic.Contains("data"))
                dic = jd = jd["data"];
            if (dic.Contains("wall"))
                AnsWall(jd);
            if (dic.Contains("floor"))
                AnsFloor(jd);
            if (dic.Contains("door"))
                AnsDoor(jd);
            if (dic.Contains("stand"))
                AnsStand(jd);
            if (dic.Contains("sky"))
                AnsSky(jd);
            if (dic.Contains("arealight"))
                AnsAreaLight(jd);
            if (dic.Contains("appliances"))
                AnsAppliances(jd);
            if (dic.Contains("flowLine"))
                AnsFlowLine(jd);
            if (dic.Contains("animal"))
                AnsAnimal(jd);
            if (dic.Contains("weather"))
                HouseWeather.Instance.SetEntity(dic["weather"]?.ToString());
            if (dic.Contains("look"))
                AnsEye(jd);
        }
    }


    private void AnsSun(string va)
    {
        float value;
        if (float.TryParse(va, out value))
        {
            if (value > 0)
            {
                var v2 = Mathf.Max(0.1f, Mathf.Sin(value * Mathf.Deg2Rad));
                HouseWeather.Instance.SetTianGuang(v2);
            }
            else
                HouseWeather.Instance.SetTianGuang(0.1f);
        }
    }

    private void TestRoom()
    {
        string wallColor = "195,216,235,255";
        CreatWall("wall1", "3.7506,-0.2897,0,0,2.433167,1,0.125", wallColor);
        CreatWall("wall1", "1.1117,2.926,0,0,2.645943,1,0.125", wallColor);
        CreatWall("wall1", "-4.1035,-4.53,0,0,0.3899986,1,0.125", wallColor);
        CreatWall("wall1", "4.1213,-4.5336,0,0,0.3512418,1,0.125", wallColor);
        CreatWall("wall1", "-0.2699,-0.26,0,0,2.158078,1,0.125", wallColor);
        CreatWall("wall1", "-4.1916,4.465,0,0,1.461849,1,0.125", wallColor);
        CreatWall("wall1", "-4.4674,-3.097,0,0,0.8893961,1,0.125", wallColor);
        CreatWall("wall1", "2.5509,0.958,0,0,0.8671593,1,0.125", wallColor);
        CreatWall("wall1", "-0.9473,0.958,0,0,2.58769,1,0.125", wallColor);
        CreatWall("wall1", "1.7963,0.958,0,0,0.7185282,0.5134,0.125", wallColor);
        CreatWall("wall1", "1.4004,-3.88,0,0,1.523783,1,0.125", wallColor);
        CreatWall("wall1", "4.5771,-3.88,0,0,0.8043213,1,0.125", wallColor);
        CreatWall("wall1", "-1.7078,-3.115,0,0,0.7367308,1,0.125", wallColor);
        CreatWall("wall1", "2.2753,-4.53,0,0,0.4198744,1,0.125", wallColor);
        CreatWall("wall1", "0.5867,-4.535,0,0,0.2674765,1,0.1349125", wallColor);
        CreatWall("wall1", "4.4384,2.926,0,0,1.012607,1,0.125", wallColor);
        CreatWall("wall1", "-1.9781,2.926,0,0,0.5189419,1,0.125", wallColor);
        CreatWall("wall1", "-1.4825,-4.53,0,0,0.8897885,1,0.125", wallColor);

        CreatWall("wall1", "-4.8955,0.6771,0,90,7.67382,1,0.125", wallColor);
        CreatWall("wall1", "0.25,1.9325,0,90,2.036761,1,0.125", wallColor);
        CreatWall("wall1", "2.179,1.9244,0,90,2.052851,1,0.125", wallColor);
        CreatWall("wall1", "4.91,-0.4659,0,90,6.907583,1,0.125", wallColor);
        CreatWall("wall1", "1.676,-2.0393,0,90,3.705132,1,0.125", wallColor);
        CreatWall("wall1", "-1.29,-2.3744,0,90,4.349825,1,0.125", wallColor);
        CreatWall("wall1", "2.105,-4.2131,0,90,0.7695358,1,0.125", wallColor);
        CreatWall("wall1", "0.674,-4.2085,0,90,0.7788795,1,0.125", wallColor);
        CreatWall("wall1", "-4.23,-3.8118,0,90,1.506382,1,0.125", wallColor);
        CreatWall("wall1", "-2.169,3.72,0,90,1.506382,1,0.125", wallColor);
        CreatWall("wall1", "4.239,-4.2102,0,90,0.7753551,1,0.125", wallColor);

        CreatFloor("groud1", "0,0,10,10,1,1", 0, null);

        CreatDoor("swingdoor1", "1.568,-0.274,0,0,0,0,-1,0.5,1,-90", "switch.xxxx", null);
        CreatDoor("swingdoor1", "1.769,-0.274,0,0,0,0,1,0.5,1,90", "switch.xxxx", null);
        CreatDoor("swingdoor1", "2.848,0.008,0,0,-90,0,1,0.5,1,0", "switch.xxxx", null);

        CreatDoor("slidingdoor1", "1.64,1.889,0,0,0,0,0.9,1,0.05,0", "switch.xxxx", "0,255,0,120");
        CreatDoor("slidingdoor1", "-2.2,2.364,0,0,-90,0,0.9,1,0.05,0", "switch.xxxx", "0,0,255,120");

        CreatStand("clock1", "0,-0.365,0.772,0,0,0,1,1,1", null);
        CreatStand("window1", "-0.3,-4.55,0,0,0,0,1,1,1", null);
        CreatStand("window1", "3.182,-4.55,0,0,0,0,1,1,1", null);
        CreatStand("window1", "3.182,2.93,0,0,180,0,1,1,1", null);
        CreatStand("window1", "-0.97,2.93,0,0,180,0,1,1,1", null);
        CreatStand("window2", "-2.923,-4.459,0,0,0,0,1,1,1", null);
        CreatStand("roundtable1", "-3.781,1.615,0,0,0,0,1,1,1", null);
        CreatStand("chair1", "-4.244,1.173,0,0,45,0,1,1,1", null);
        CreatStand("chair1", "-3.318,1.172,0,0,-45,0,1,1,1", null);
        CreatStand("chair1", "-4.237,2.067,0,0,135,0,1,1,1", null);
        CreatStand("chair1", "-3.34,2.07,0,0,-135,0,1,1,1", null);
        CreatStand("tree", "-1.66,-0.27,0,0,0,0,1,1,1", null);
        CreatStand("tvstand", "-1.64,-1.525,0,0,-90,0,2,0.65,0.5", null);
        CreatStand("bed1", "-0.202,-2.24,0,0,90,0,1,1,1", null);
        CreatStand("bed1", "3.811,-2.24,0,0,-90,0,1,1,1", null);
        CreatStand("bed1", "3.2,2.12,0,0,90,0,0.855,1,0.95", null);
        CreatStand("shelving", "-0.3542,-0.644,0,0,-90,0,1.218,1,3.35", null);
        CreatStand("shelving", "3.773,-0.64,0,0,-90,0,1.218,1,3.98", null);
        CreatStand("shelving", "4.48,1.27,0,0,0,0,1.218,1,5.69", null);
        CreatStand("shelving", "-1.24,2.5,0,0,-90,0,1.218,1,3.32", null);
        CreatStand("shelving", "-0.115,1.97,0,0,0,0,1.218,1,3.35", null);

        CreatStand("yushigui", "2.11,1.44,0,0,270,0,0.65,0.65,0.65", null);
        CreatStand("sofa3", "-4.35,-1.25,0,0,0,0,0.75,0.75,0.65", null);
        CreatStand("wc2", "2.11,2.35,0,0,270,0,1,1,1", null);
        CreatStand("mirror", "2.11,1.318,0.613,0,-90,0,0.6,0.57,1", null);

        CreatFloor("wood", "0.1962,-2.06,2.77,3.418,0.5,0.5", 1, null);
        CreatFloor("wood", "3.304,-2.02,3.04,3.5,0.5,0.5", 1, null);
        CreatFloor("wood", "3.195,-4.10,1.928,0.67,0.1,0.1", 1, null);
        CreatFloor("wood", "-0.3,-4.11,1.79,0.69,0.1,0.1", 1, null);
        CreatFloor("wood", "3.9,0.44,1.857,1.24,0.5,0.5", 1, null);
        CreatFloor("wood", "3.54,1.96,2.53,1.8,0.5,0.5", 1, null);
        CreatFloor("tile4", "-3.516,0.652,2.539,7.36,1,4", 1, null);
        CreatFloor("tile4", "-2.752,-3.747,2.8,1.44,1.5,1", 1, null);
        CreatFloor("tile4", "0.264,0.355,5.1,1.06,2,0.5", 2, null);
        CreatFloor("tile4", "-1.81,-1.6,0.89,2.86,0.4,1.5", 2, null);
        CreatFloor("tile4", "-1.02,1.94,2.47,1.84,1.1,1.1", 1, null);
        CreatFloor("tile4", "1.2,1.35,1.785,0.936,0.8,0.4", 1, null);

        CreatAnim("zebra", "-3.516,0.652,2.539,7.36");

        CreatAreaLight("-3.55,2.08,3,3,3", "light.xxx", null);
        CreatAreaLight("-2.958,-1.49,3,3,5", "light.xxx", null);
        CreatAreaLight("0.41,-2.24,2,3,5", "light.xxx", null);
        CreatAreaLight("3.27,-2.3,2,3,4", "light.xxx", null);
        CreatAreaLight("3.55,1.85,2,2,4", "light.xxx", null);
        CreatAreaLight("-1,1.93,2,2,2", "light.xxx", null);
        CreatAreaLight("0.593,0.33,4,0.8,5", "light.xxx", "178,149,70,255");
        CreatAreaLight("1.22,2.4,1.79,0.8,2", "light.xxx", null);

        //CreatFlowLine("-3.614539,4.459235,-1.1,-4.868434,4.454682,-1.1,-4.895442,-3.080526,-1.1,-4.211635,-3.101046,-1.1,-4.195337,-4.541389,-1.1,-1.315552,-4.536458,-1.1,0.6916895,-4.544263,-1.1,0.7049857,-3.88601,-1.1,2.096153,-3.88601,-1.1,2.096153,-4.559184,-1.1,4.226263,-4.559184,-1.1,4.226263,-3.867848,-1.1,4.944883,-3.867568,-1.1,4.896365,2.945096,-1.1,-2.157866,2.945096,-1.1,-2.157866,4.409803,-1.1",
        //                             -1, "255,255,255,255", "red", "switch.xxxx"));
        CreatFlowLine("-3.979907,3.786215,1,-3.979907,3.782755,0.1,-3.162498,3.782755,0.1,-3.162498,0.4754077,0.1,1.302327,0.4013318,0.1,1.296336,-1.111547,0.1,-0.9975456,-1.101645,0.1,-1.026982,-3.49305,0.1,1.298337,-3.513381,0.1",
            -1, "0,165,255,255", "red", "");
        CreatFlowLine("1.34175,0.4129494,0.1,2.299839,0.4083416,0.1,2.300617,-1.182749,0.1,4.609478,-1.177224,0.1,4.586144,-3.498736,0.1,2.178244,-3.516147,0.1",
            -1, "255,210,0,255", "red", "");
        CreatFlowLine("1.34175,0.5860035,0.1,3.917953,0.5754554,0.1,3.923838,1.181043,0.1,2.362754,1.187165,0.1",
            -1, "0,255,223,255", "red", "");
        CreatFlowLine("-1.931547,0.3116443,0.1,-1.901711,-2.992601,0.1,-1.968843,-2.982793,1.1,-1.968843,-3.266867,1.1,-1.968843,-3.267096,0.1,-1.968843,-3.869098,0.1",
            -1, "0,255,223,255", "red", "");
        CreatFlowLine("-2.123267,-2.820107,0.1,-4.781082,-2.769531,0.1,-4.776298,-2.776958,0.9,-4.732517,0.1199159,0.9,-4.721735,0.1224415,0.3",
            -1, "255,210,0,255", "red", "");
        CreatFlowLine("-3.035549,2.620526,0.1,0.1451162,2.608722,0.1,0.1221144,2.625938,1.1,1.986478,2.625938,1.1,2.006577,1.321079,1.1,1.997306,1.308576,0.9",
            -1, "255,0,252,255", "red", "");
        CreatStand("electricball", "-3.986,3.766,1.085,0,0,0,0.4,0.4,0.4", null);
        HouseWeather.Instance.CreatSky("space");
        CreatAppliances("tv", "-1.56,-1.565,0.41,0,-90,0,0.7,0.7,0.7", "switch.xxx", null, null);
        CreatAppliances("washer", "-1.68,-4.166,0,0,-90,0,1,1,1", "switch.xxx", null, null);
    }
    /// <summary>xywh </summary>
    public async Task<HassEntity> CreatAnim(string cusname, string str)
    {
        if (string.IsNullOrEmpty(str)) return null;
        var ss = str.Split(',');
        if (ss.Length != 4) return null;
        float va, x, y, w, h;
        if (float.TryParse(ss[0], out va)) x = va; else return null;
        if (float.TryParse(ss[1], out va)) y = va; else return null;
        if (float.TryParse(ss[2], out va)) w = va; else return null;
        if (float.TryParse(ss[3], out va)) h = va; else return null;

        await Help.Instance.ABLoad("animal", cusname);
        var ab = Help.Instance.GetBundle("animal", cusname);
        if (!ab) return null;
        var tr = ab.LoadAsset<GameObject>(cusname);
        tr = Instantiate(tr, parent);
        tr.name = cusname;
        var area = new GameObject().transform;
        area.transform.SetParent(parent);
        area.position = new Vector3(x, 0, y);
        area.localScale = new Vector3(w, 0, h);
        var animal = tr.GetComponent<ZebraEntity>();
        animal.SetArea(area);
        cureetHouse.Add(tr.transform, HouseEntityType.animal);
        return animal;
    }

    /// <summary>posxzy anglexyz scalexyz </summary>
    public async Task<HassEntity> CreatAppliances(string cusname, string str, string id, string more, string color)
    {
        if (string.IsNullOrEmpty(str)) return null;
        var ss = str.Split(',');
        if (ss.Length == 9)
        {
            float va, x, y, z, rx, ry, rz, sx, sy, sz;
            if (float.TryParse(ss[0], out va)) x = va; else return null;
            if (float.TryParse(ss[1], out va)) y = va; else return null;
            if (float.TryParse(ss[2], out va)) z = va; else return null;
            if (float.TryParse(ss[3], out va)) rx = va; else return null;
            if (float.TryParse(ss[4], out va)) ry = va; else return null;
            if (float.TryParse(ss[5], out va)) rz = va; else return null;
            if (float.TryParse(ss[6], out va)) sx = va; else return null;
            if (float.TryParse(ss[7], out va)) sy = va; else return null;
            if (float.TryParse(ss[8], out va)) sz = va; else return null;
            var tr = await CreatAppliances(cusname, id);
            if (tr)
            {
                tr.transform.position = new Vector3(x, z, y);
                tr.transform.eulerAngles = new Vector3(rx, ry, rz);
                tr.transform.localScale = new Vector3(sx, sy, sz);
            }
            if (tr is TextEntity)
            {
                var e = tr as TextEntity;
                if (!string.IsNullOrEmpty(more))
                {
                    ss = more.Split(',');
                    if (ss.Length == 2)
                    {
                        e.front = ss[0];
                        e.back = ss[1];
                    }
                }
                Color c;
                if (Help.Instance.TryColor(color, out c))
                    e.textMesh.color = c;
            }
            else if (tr is AlarmEntity)
            {
                var e = tr as AlarmEntity;
                Color c;
                if (Help.Instance.TryColor(color, out c))
                    e.YuanMa.SetColor("_Color", c);
            }
            else if (tr is LampEntity)
            {
                var e = tr as LampEntity;
                Color c;
                if (Help.Instance.TryColor(color, out c))
                    e.emColor = c;
            }
        }
        return null;
    }
    public async Task<HassEntity> CreatAppliances(string cusname, string id)
    {
        await Help.Instance.ABLoad("appliances", cusname);
        var ab = Help.Instance.GetBundle("appliances", cusname);
        if (ab == null) return null;
        var tr = ab.LoadAsset<GameObject>(cusname);
        tr = Instantiate(tr, parent);
        tr.name = cusname;
        var e = tr.GetComponent<HassEntity>();
        if (e)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                e.SetEntity(id);
                lis.Add(e);
            }
        }
        cureetHouse.Add(tr.transform, HouseEntityType.appliances);
        return e;
    }
    /// <summary>posxzy anglexyz scalexyz </summary>
    public async Task<Transform> CreatStand(string cusname, string str, string color)
    {
        if (string.IsNullOrEmpty(str)) return null;
        var ss = str.Split(',');
        if (ss.Length == 9)
        {
            float va, x, y, z, rx, ry, rz, sx, sy, sz;
            if (float.TryParse(ss[0], out va)) x = va; else return null;
            if (float.TryParse(ss[1], out va)) y = va; else return null;
            if (float.TryParse(ss[2], out va)) z = va; else return null;
            if (float.TryParse(ss[3], out va)) rx = va; else return null;
            if (float.TryParse(ss[4], out va)) ry = va; else return null;
            if (float.TryParse(ss[5], out va)) rz = va; else return null;
            if (float.TryParse(ss[6], out va)) sx = va; else return null;
            if (float.TryParse(ss[7], out va)) sy = va; else return null;
            if (float.TryParse(ss[8], out va)) sz = va; else return null;

            var tr = await CreatStand(cusname, color);
            if (tr)
            {
                tr.transform.position = new Vector3(x, z, y);
                tr.transform.eulerAngles = new Vector3(rx, ry, rz);
                tr.transform.localScale = new Vector3(sx, sy, sz);
            }
        }
        return null;
    }

    /// <summary>posxzy anglexyz scalexyz </summary>
    public async Task<Transform> CreatStand(string cusname, string color)
    {
        await Help.Instance.ABLoad("stand", cusname);
        var ab = Help.Instance.GetBundle("stand", cusname);
        if (ab == null) return null;
        var tr = ab.LoadAsset<GameObject>(cusname);
        tr = Instantiate(tr, parent);
        tr.name = cusname;
        cureetHouse.Add(tr.transform, HouseEntityType.stand);
        var collider = tr.GetComponent<Collider>();
        if (collider) collider.enabled = false;
        return tr.transform;
    }

    public async Task<Eye> CreatEye(string cusname, string pos, string priority, string text)
    {
        if (string.IsNullOrEmpty(pos)) return null;
        var ss = pos.Split(',');
        if (ss.Length == 9)
        {
            float va, x, y, z, rx, ry, rz, sx, sy, sz;
            if (float.TryParse(ss[0], out va)) x = va; else return null;
            if (float.TryParse(ss[1], out va)) y = va; else return null;
            if (float.TryParse(ss[2], out va)) z = va; else return null;
            if (float.TryParse(ss[3], out va)) rx = va; else return null;
            if (float.TryParse(ss[4], out va)) ry = va; else return null;
            if (float.TryParse(ss[5], out va)) rz = va; else return null;
            if (float.TryParse(ss[6], out va)) sx = va; else return null;
            if (float.TryParse(ss[7], out va)) sy = va; else return null;
            if (float.TryParse(ss[8], out va)) sz = va; else return null;

            var tr = await CreatEye(cusname);
            if (tr)
            {
                tr.transform.position = new Vector3(x, z, y);
                tr.transform.eulerAngles = new Vector3(rx, ry, rz);
                tr.transform.localScale = new Vector3(sx, sy, sz);
            }
            var e = tr.GetComponent<Eye>();
            int.TryParse(priority, out e.priority);
            e.textMesh.text = text;
        }
        return null;
    }

    public async Task<Transform> CreatEye(string cusname)
    {
        await Help.Instance.ABLoad("look", cusname);
        var ab = Help.Instance.GetBundle("look", cusname);
        if (ab == null) return null;
        var tr = ab.LoadAsset<GameObject>(cusname);
        tr = Instantiate(tr, parent);
        tr.name = cusname;
        cureetHouse.Add(tr.transform, HouseEntityType.eye);
        return tr.transform;
    }

    public async Task<LineEntity> CreatFlowLine(string pos, int speed, string con, string coff, string id)
    {
        var vs = GetPoss(pos);
        if (vs == null) return null;
        await Help.Instance.ABLoad("line", "flowline");
        var ab = Help.Instance.GetBundle("line", "flowline");
        if (!ab) return null;
        var tr = ab.LoadAsset<GameObject>("flowline");
        var lineE = Instantiate(tr, parent).GetComponent<LineEntity>();
        lineE.name = "line";

        lineE.line.positionCount = vs.Length;
        lineE.line.SetPositions(vs);
        Color c;
        if (!string.IsNullOrWhiteSpace(id))
        {
            lineE.SetEntity(id);
            lis.Add(lineE);
        }
        if (Help.Instance.TryColor(con, out c))
            lineE.SetOnc(c);
        if (Help.Instance.TryColor(coff, out c))
            lineE.SetOffc(c);
        cureetHouse.Add(lineE.transform, HouseEntityType.flowLine);
        var ma = lineE.line.material;
        ma = new Material(ma);
        ma.SetFloat("_Speed", speed);
        lineE.line.material = ma;
        return lineE;
    }

    Vector3[] GetPoss(string str)
    {
        if (string.IsNullOrEmpty(str)) return null;
        var ss = str.Split(',');
        if (ss.Length % 3 == 0)
        {
            Vector3[] vs = new Vector3[ss.Length / 3];
            var n = 0;
            float va, x, y, z;
            for (int i = 0; i < ss.Length; i += 3)
            {
                if (float.TryParse(ss[i], out va)) x = va; else return null;
                if (float.TryParse(ss[i + 1], out va)) z = va; else return null;
                if (float.TryParse(ss[i + 2], out va)) y = va; else return null;
                vs[n] = new Vector3(x, y, z);
                n++;
            }
            return vs;
        }
        return null;
    }

    /// <summary>posxzy anglexyz scalexyz open</summary>
    public async Task<DoorEntity> CreatDoor(string cusname, string str, string id, string color)
    {
        if (string.IsNullOrEmpty(str)) return null;
        var ss = str.Split(',');
        if (ss.Length == 10)
        {
            float va, x, y, z, rx, ry, rz, sx, sy, sz, open;
            if (float.TryParse(ss[0], out va)) x = va; else return null;
            if (float.TryParse(ss[1], out va)) y = va; else return null;
            if (float.TryParse(ss[2], out va)) z = va; else return null;
            if (float.TryParse(ss[3], out va)) rx = va; else return null;
            if (float.TryParse(ss[4], out va)) ry = va; else return null;
            if (float.TryParse(ss[5], out va)) rz = va; else return null;
            if (float.TryParse(ss[6], out va)) sx = va; else return null;
            if (float.TryParse(ss[7], out va)) sy = va; else return null;
            if (float.TryParse(ss[8], out va)) sz = va; else return null;
            if (float.TryParse(ss[9], out va)) open = va; else return null;

            await Help.Instance.ABLoad("door", cusname);
            var ab = Help.Instance.GetBundle("door", cusname);
            if (ab == null) return null;
            var tr = ab.LoadAsset<GameObject>(cusname);
            tr = Instantiate(tr, parent);
            tr.name = cusname;
            var p = new Vector3(x, z, y);
            tr.transform.position = p;
            var a = new Vector3(rx, ry, rz);
            tr.transform.eulerAngles = a;
            var s = new Vector3(sx, sy, sz);
            tr.transform.localScale = s;
            var le = tr.GetComponent<DoorEntity>();
            if (!string.IsNullOrWhiteSpace(id))
            {
                le.SetEntity(id);
                lis.Add(le);
            }
            cureetHouse.Add(tr.transform, HouseEntityType.door);
            le.angleopen = open;
            Color sc;
            if (Help.Instance.TryColor(color, out sc))
            {
                var mr = tr.GetComponent<MeshRenderer>();
                if (!mr) return null;
                var ma = new Material(mr.material);
                ma.color = sc;
                mr.material = ma;
            }
            var collider = tr.GetComponent<Collider>();
            if (collider) collider.enabled = false;
            return le;
        }
        return null;
    }

    /// <summary>x y w h li </summary>
    public LightEntity CreatAreaLight(string str, string id, string color)
    {
        if (string.IsNullOrEmpty(str)) return null;
        var ss = str.Split(',');
        if (ss.Length == 5)
        {
            float x, y, w, h, li;
            float va;
            if (float.TryParse(ss[0], out va)) x = va; else return null;
            if (float.TryParse(ss[1], out va)) y = va; else return null;
            if (float.TryParse(ss[2], out va)) w = va; else return null;
            if (float.TryParse(ss[3], out va)) h = va; else return null;
            if (float.TryParse(ss[4], out va)) li = va; else return null;
            //yield return Help.Instance.ABLoad("light", "arealight");
            //var ab = Help.Instance.GetBundle("light", "arealight");
            //var tr = ab.LoadAsset<GameObject>("arealight");
            var prefab = transform.Find("arealight");
            var tr = Instantiate(prefab, parent);
            tr.name = "light";
            tr.transform.position = new Vector3(x, 0.01f, y);
            tr.transform.localScale = new Vector3(w, 0.1f, h);
            var le = tr.GetComponent<LightEntity>();
            tr.gameObject.SetActive(true);
            if (!string.IsNullOrWhiteSpace(id))
            {
                le.SetEntity(id);
                lis.Add(le);
            }
            cureetHouse.Add(tr.transform, HouseEntityType.arealight);
            le.clight.intensity = li;
            Color c;
            if (Help.Instance.TryColor(color, out c))
            {
                le.clight.color = c;
            }
            return le;
        }
        return null;
    }

    /// <summary>x,y w,h tx,ty</summary>
    public async Task<Transform> CreatFloor(string cusname, string str, int priority, string color)
    {
        if (string.IsNullOrEmpty(str)) return null;
        var ss = str.Split(',');
        if (ss.Length != 6) return null;
        float va, x, y, w, h, tx, ty;
        if (float.TryParse(ss[0], out va)) x = va; else return null;
        if (float.TryParse(ss[1], out va)) y = va; else return null;
        if (float.TryParse(ss[2], out va)) w = va * 0.1f; else return null;
        if (float.TryParse(ss[3], out va)) h = va * 0.1f; else return null;
        if (float.TryParse(ss[4], out va)) tx = va; else return null;
        if (float.TryParse(ss[5], out va)) ty = va; else return null;

        await Help.Instance.ABLoad("floor", cusname);
        var ab = Help.Instance.GetBundle("floor", cusname);
        if (!ab) return null;
        var tr = ab.LoadAsset<GameObject>(cusname);
        tr = Instantiate(tr, parent);
        tr.name = cusname;
        tr.transform.position = new Vector3(x, priority * 0.01f, y);
        tr.transform.localScale = new Vector3(w, 1, h);
        var ma = tr.GetComponent<MeshRenderer>().material;
        ma = new Material(ma);
        ma.SetTextureScale("_BaseMap", new Vector2(tx, ty));
        Color c;
        if (Help.Instance.TryColor(color, out c))
            ma.color = c;
        tr.GetComponent<MeshRenderer>().material = ma;
        cureetHouse.Add(tr.transform, HouseEntityType.floor);
        var collider = tr.GetComponent<Collider>();
        if (collider) collider.enabled = false;
        return tr.transform;
    }

    /// <summary>posxzy a wht  </summary>
    public async Task<Transform> CreatWall(string cusname, string wall, string color)
    {
        if (string.IsNullOrEmpty(wall)) return null;
        var ws = wall.Split(',');
        float va;
        if (ws.Length != 7) return null;
        float x, y, z, w, h, t, a;
        if (float.TryParse(ws[0], out va)) x = va; else return null;
        if (float.TryParse(ws[1], out va)) y = va; else return null;
        if (float.TryParse(ws[2], out va)) z = va; else return null;
        if (float.TryParse(ws[3], out va)) a = va; else return null;
        if (float.TryParse(ws[4], out va)) w = va; else return null;
        if (float.TryParse(ws[5], out va)) h = va; else return null;
        if (float.TryParse(ws[6], out va)) t = va; else return null;

        await Help.Instance.ABLoad("wall", cusname);
        var ab = Help.Instance.GetBundle("wall", cusname);
        if (!ab) return null;
        var tr = ab.LoadAsset<GameObject>(cusname);
        var cube = Instantiate(tr, parent);
        cube.name = tr.name;
        cube.transform.SetParent(parent);
        cube.transform.localPosition = new Vector3(x, z, y);
        cube.transform.localScale = new Vector3(w, h, t);
        cube.transform.localEulerAngles = new Vector3(0, a, 0);
        var r = cube.GetComponent<MeshRenderer>();
        r.material = new Material(r.material);
        Color wcolor;
        if (Help.Instance.TryColor(color, out wcolor))
        {
            r.material.color = wcolor;
        }
        cureetHouse.Add(cube.transform, HouseEntityType.wall);
        var collider = tr.GetComponent<Collider>();
        if (collider) collider.enabled = false;
        return cube.transform;
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
        if (Reconstitution.Instance.IsEdit)
            return;
        if (Physics.Raycast(mainCa.ScreenPointToRay(Input.mousePosition), out hit, maxDistance))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            if (hit.transform != lasthit)
            {
                lasthit = hit.transform;
                if (entity)
                    entity.MouseExit();
                entity = hit.transform.GetComponent<HassEntity>();
                if (entity)
                    entity.MouseOn();
            }
            if (entity)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    lastDownpos = Input.mousePosition;
                    lastDownT = Time.time;
                }
                else if (Input.GetMouseButton(0))
                {
                    if (Time.time - lastDownT > 1 && Vector3.Magnitude(Input.mousePosition - lastDownpos) < 9)
                    {
                        entity.LongClick();
                        lastDownT = Time.time + 1000;
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    if (Mathf.Abs(Time.time - lastDownT) < 0.5f && Vector3.Magnitude(Input.mousePosition - lastDownpos) < 9)
                    {
                        entity.Click();
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
        if (CameraControllerForUnity.Instance.mode == CameraControllerForUnity.Mode.third)
        {
            CameraControllerForUnity.Instance.xAngle += Time.deltaTime * HousePanel.Instance.RoSpeed;
            if (CameraControllerForUnity.Instance.xAngle > 360)
                CameraControllerForUnity.Instance.xAngle -= 360;
        }
    }

    private void AnsFloor(JsonData jd)
    {
        var doors = jd["floor"];
        if (doors == null || !doors.IsArray) return;
        doors.Foreach((cusname, door) =>
        {
            if (door == null) return;
            cusname = door.Prop_Name;
            if (cusname == null) return;
            var canshu = door[cusname];
            var dic = canshu as IDictionary;
            if (dic == null) return;
            string pos = null, color = null;
            int priority = 0;
            if (dic.Contains("pos"))
                pos = dic["pos"]?.ToString();
            if (dic.Contains("priority"))
                int.TryParse(dic["priority"]?.ToString(), out priority);
            if (dic.Contains("color"))
                color = dic["color"]?.ToString();
            CreatFloor(cusname, pos, priority, color);
        });
    }

    private void AnsFlowLine(JsonData jd)
    {
        var doors = jd["flowLine"];
        if (doors == null || !doors.IsArray) return;
        doors.Foreach((cusname, door) =>
        {
            if (door == null) return;
            cusname = door.Prop_Name;
            if (cusname == null) return;
            var canshu = door[cusname];
            var dic = canshu as IDictionary;
            if (dic == null) return;
            string pos = null, entity = null, con = null, coff = null;
            int speed = 1;
            if (dic.Contains("pos"))
                pos = dic["pos"]?.ToString();
            if (dic.Contains("entity"))
                entity = dic["entity"]?.ToString();
            if (dic.Contains("con"))
                con = dic["con"]?.ToString();
            if (dic.Contains("coff"))
                coff = dic["coff"]?.ToString();
            if (dic.Contains("speed"))
                int.TryParse(dic["speed"]?.ToString(), out speed);
            CreatFlowLine(pos, speed, con, coff, entity);
        });
    }
    private void AnsAreaLight(JsonData jd)
    {
        var doors = jd["arealight"];
        if (doors == null || !doors.IsArray) return;
        doors.Foreach((cusname, door) =>
        {
            if (door == null) return;
            cusname = door.Prop_Name;
            if (cusname == null) return;
            var canshu = door[cusname];
            var dic = canshu as IDictionary;
            if (dic == null) return;
            string pos = null, entity = null, color = null;
            if (dic.Contains("pos"))
                pos = dic["pos"]?.ToString();
            if (dic.Contains("entity"))
                entity = dic["entity"]?.ToString();
            if (dic.Contains("color"))
                color = dic["color"]?.ToString();
            CreatAreaLight(pos, entity, color);
        });
    }

    private void AnsAppliances(JsonData jd)
    {
        var doors = jd["appliances"];
        if (doors == null || !doors.IsArray) return;
        doors.Foreach((cusname, door) =>
        {
            if (door == null) return;
            cusname = door.Prop_Name;
            if (cusname == null) return;
            var canshu = door[cusname];
            var dic = canshu as IDictionary;
            if (dic == null) return;
            string pos = null, entity = null, color = null, more = null;
            if (dic.Contains("pos"))
                pos = dic["pos"]?.ToString();
            if (dic.Contains("entity"))
                entity = dic["entity"]?.ToString();
            if (dic.Contains("more"))
                more = dic["more"]?.ToString();
            if (dic.Contains("color"))
                color = dic["color"]?.ToString();
            CreatAppliances(cusname, pos, entity, more, color);
        });
    }
    private void AnsSky(JsonData jd)
    {
        var sky = jd["sky"]?.ToString();
        HouseWeather.Instance.CreatSky(sky);
    }

    private void AnsStand(JsonData jd)
    {
        var doors = jd["stand"];
        if (doors == null || !doors.IsArray) return;
        doors.Foreach((cusname, door) =>
        {
            if (door == null) return;
            cusname = door.Prop_Name;
            if (cusname == null) return;
            var canshu = door[cusname];
            var dic = canshu as IDictionary;
            if (dic == null) return;
            string pos = null, color = null;
            if (dic.Contains("pos"))
                pos = dic["pos"]?.ToString();
            if (dic.Contains("color"))
                color = dic["color"]?.ToString();
            CreatStand(cusname, pos, color);
        });
    }
    private void AnsEye(JsonData jd)
    {
        var doors = jd["look"];
        if (doors == null || !doors.IsArray) return;
        doors.Foreach((cusname, door) =>
        {
            if (door == null) return;
            cusname = door.Prop_Name;
            if (cusname == null) return;
            var canshu = door[cusname];
            var dic = canshu as IDictionary;
            if (dic == null) return;
            string pos = null, priority = null, text = null;
            if (dic.Contains("pos"))
                pos = dic["pos"]?.ToString();
            if (dic.Contains("priority"))
                priority = dic["priority"]?.ToString();
            if (dic.Contains("text"))
                text = dic["text"]?.ToString();
            CreatEye(cusname, pos, priority, text);
        });
    }


    private void AnsDoor(JsonData jd)
    {
        var doors = jd["door"];
        if (doors == null || !doors.IsArray) return;
        doors.Foreach((cusname, door) =>
        {
            if (door == null) return;
            cusname = door.Prop_Name;
            if (cusname == null) return;
            var canshu = door[cusname];
            var dic = canshu as IDictionary;
            if (dic == null) return;
            string pos = null, color = null, entity = null;
            if (dic.Contains("pos"))
                pos = dic["pos"]?.ToString();
            if (dic.Contains("color"))
                color = dic["color"]?.ToString();
            if (dic.Contains("entity"))
                entity = dic["entity"]?.ToString();
            CreatDoor(cusname, pos, entity, color);
        });
    }
    private void AnsAnimal(JsonData jd)
    {
        var doors = jd["animal"];
        if (doors == null || !doors.IsArray) return;
        doors.Foreach((cusname, door) =>
        {
            if (door == null) return;
            cusname = door.Prop_Name;
            if (cusname == null) return;
            var canshu = door[cusname];
            var dic = canshu as IDictionary;
            if (dic == null) return;
            string pos = null;
            if (dic.Contains("pos"))
                pos = dic["pos"]?.ToString();
            CreatAnim(cusname, pos);
        });
    }
    private void AnsWall(JsonData jd)
    {
        var walls = jd["wall"];
        if (walls == null || !walls.IsArray) return;
        walls.Foreach((cusname, wall) =>
        {
            if (wall == null) return;
            cusname = wall.Prop_Name;
            if (cusname == null) return;
            var canshu = wall[cusname];
            var dic = canshu as IDictionary;
            if (dic == null) return;
            string pos = null, color = null;
            if (dic.Contains("pos"))
                pos = dic["pos"]?.ToString();
            if (dic.Contains("color"))
                color = dic["color"]?.ToString();
            CreatWall(cusname, pos, color);
        });
    }

    public string House2Json()
    {
        var hs = from a in cureetHouse
                 group a.Key by a.Value;
        JsonWriter writer = new JsonWriter();
        writer.WriteObjectStart();
        foreach (var group in hs)
        {
            switch (group.Key)
            {
                case HouseEntityType.wall:
                    Wall2Json(writer, group);
                    break;
                case HouseEntityType.floor:
                    Floor2Json(writer, group);
                    break;
                case HouseEntityType.door:
                    Door2Json(writer, group);
                    break;
                case HouseEntityType.stand:
                    Stand2Json(writer, group);
                    break;
                case HouseEntityType.sky:
                    break;
                case HouseEntityType.arealight:
                    Arealight2Json(writer, group);
                    break;
                case HouseEntityType.appliances:
                    App2Json(writer, group);
                    break;
                case HouseEntityType.flowLine:
                    Line2Josn(writer, group);
                    break;
                case HouseEntityType.animal:
                    Animal2Json(writer, group);
                    break;
                case HouseEntityType.weather:
                    Weather2Json(writer, group);
                    break;
                case HouseEntityType.eye:
                    Eye2Json(writer, group);
                    break;
                default:
                    break;
            }

        }
        if (!string.IsNullOrWhiteSpace(HouseWeather.Instance.Entity_id))
        {
            writer.WritePropertyName("weather");
            writer.Write(HouseWeather.Instance.Entity_id);
        }
        if (!string.IsNullOrWhiteSpace(HouseWeather.Instance.Skyname))
        {
            writer.WritePropertyName("sky");
            writer.Write(HouseWeather.Instance.Skyname);
        }
        writer.WriteObjectEnd();
        return writer.ToString();
    }

    private void Weather2Json(JsonWriter writer, IGrouping<HouseEntityType, Transform> group)
    {
        writer.WritePropertyName("weather");
        writer.Write(HouseWeather.Instance.Entity_id);
    }

    private void Animal2Json(JsonWriter writer, IGrouping<HouseEntityType, Transform> group)
    {
        writer.WritePropertyName("animal");
        writer.WriteArrayStart();
        foreach (var g in group)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName(g.name);
            writer.WriteObjectStart();
            var anim = g.GetComponent<ZebraEntity>();
            writer.WritePropertyName("pos");
            var item = anim.Area;
            writer.Write($"{item.position.x:f2},{item.position.z:f2},{item.localScale.x:f2},{item.localScale.z:f2}");
            writer.WriteObjectEnd();
            writer.WriteObjectEnd();
        }
        writer.WriteArrayEnd();
    }

    private void Line2Josn(JsonWriter writer, IGrouping<HouseEntityType, Transform> group)
    {
        writer.WritePropertyName("flowLine");
        writer.WriteArrayStart();
        foreach (var item in group)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName(item.name);
            writer.WriteObjectStart();
            var line = item.GetComponent<LineEntity>();
            writer.WritePropertyName("pos");
            Vector3[] v3s = new Vector3[line.line.positionCount];
            line.line.GetPositions(v3s);
            string str = string.Empty;
            foreach (var p in v3s)
                str += $"{p.x:f2},{p.z:f2},{p.y:f2},";
            str = str.TrimEnd(',');
            writer.Write(str);
            writer.WritePropertyName("con");
            Color32 ma = line.Oncolor;
            writer.Write($"{ma.r},{ma.g},{ma.b},{ma.a}");
            writer.WritePropertyName("coff");
            ma = line.Offcolor;
            writer.Write($"{ma.r},{ma.g},{ma.b},{ma.a}");
            writer.WritePropertyName("speed");
            var s = line.line.material.GetFloat("_Speed");
            writer.Write(s);
            writer.WritePropertyName("entity");
            writer.Write(line.Entity_id);
            writer.WriteObjectEnd();
            writer.WriteObjectEnd();
        }
        writer.WriteArrayEnd();
    }

    private void App2Json(JsonWriter writer, IGrouping<HouseEntityType, Transform> group)
    {
        writer.WritePropertyName("appliances");
        writer.WriteArrayStart();
        foreach (var item in group)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName(item.name);
            writer.WriteObjectStart();
            writer.WritePropertyName("pos");
            writer.Write($"{item.position.x:f2},{item.position.z:f2},{item.position.y:f2},{item.eulerAngles.x:f2},{item.eulerAngles.y:f2},{item.eulerAngles.z:f2},{item.localScale.x:f2},{item.localScale.y:f2},{item.localScale.z:f2}");
            writer.WritePropertyName("entity");
            var tr = item.GetComponent<HassEntity>();
            writer.Write(tr.Entity_id);
            if (tr is AlarmEntity)
            {
                var e = tr as AlarmEntity;
                writer.WritePropertyName("color");
                Color32 ma = e.YuanMa.GetColor("_Color");
                writer.Write($"{ma.r},{ma.g},{ma.b},{ma.a}");
            }
            else if (tr is TextEntity)
            {
                var e = tr as TextEntity;
                writer.WritePropertyName("color");
                Color32 ma = e.textMesh.color;
                writer.Write($"{ma.r},{ma.g},{ma.b},{ma.a}");
                writer.WritePropertyName("more");
                writer.Write($"{e.front},{e.back}");
            }
            else if (tr is LampEntity)
            {
                var e = tr as LampEntity;
                writer.WritePropertyName("color");
                Color32 ma = e.emColor;
                writer.Write($"{ma.r},{ma.g},{ma.b},{ma.a}");
            }
            writer.WriteObjectEnd();
            writer.WriteObjectEnd();
        }
        writer.WriteArrayEnd();
    }

    private void Arealight2Json(JsonWriter writer, IGrouping<HouseEntityType, Transform> group)
    {
        writer.WritePropertyName("arealight");
        writer.WriteArrayStart();
        foreach (var item in group)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName(item.name);
            writer.WriteObjectStart();
            var lit = item.GetComponent<LightEntity>();
            writer.WritePropertyName("pos");
            writer.Write($"{item.position.x:f2},{item.position.z:f2},{item.localScale.x:f2},{item.localScale.z:f2},{lit.Max:f2}");
            writer.WritePropertyName("color");
            Color32 ma = lit.clight.color;
            writer.Write($"{ma.r},{ma.g},{ma.b},{ma.a}");
            writer.WritePropertyName("entity");
            writer.Write(lit.Entity_id);
            writer.WriteObjectEnd();
            writer.WriteObjectEnd();
        }
        writer.WriteArrayEnd();
    }

    private void Stand2Json(JsonWriter writer, IGrouping<HouseEntityType, Transform> group)
    {
        writer.WritePropertyName("stand");
        writer.WriteArrayStart();
        foreach (var item in group)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName(item.name);
            writer.WriteObjectStart();
            writer.WritePropertyName("pos");
            writer.Write($"{item.position.x:f2},{item.position.z:f2},{item.position.y:f2},{item.eulerAngles.x:f2},{item.eulerAngles.y:f2},{item.eulerAngles.z:f2},{item.localScale.x:f2},{item.localScale.y:f2},{item.localScale.z:f2}");
            //writer.WritePropertyName("color");
            //Color32 ma = item.GetComponent<MeshRenderer>().material.color;
            //writer.Write($"{ma.r},{ma.g},{ma.b},{ma.a}");
            writer.WriteObjectEnd();
            writer.WriteObjectEnd();
        }
        writer.WriteArrayEnd();
    }

    private void Door2Json(JsonWriter writer, IGrouping<HouseEntityType, Transform> group)
    {
        writer.WritePropertyName("door");
        writer.WriteArrayStart();
        foreach (var item in group)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName(item.name);
            writer.WriteObjectStart();
            var door = item.GetComponent<DoorEntity>();
            writer.WritePropertyName("pos");
            writer.Write($"{item.position.x:f2},{item.position.z:f2},{item.position.y:f2},{item.eulerAngles.x:f2},{item.eulerAngles.y:f2},{item.eulerAngles.z:f2},{item.localScale.x:f2},{item.localScale.y:f2},{item.localScale.z:f2},{door.angleopen}");
            writer.WritePropertyName("color");
            Color32 ma = item.GetComponent<MeshRenderer>().material.color;
            writer.Write($"{ma.r},{ma.g},{ma.b},{ma.a}");
            writer.WritePropertyName("entity");
            writer.Write(door.Entity_id);
            writer.WriteObjectEnd();
            writer.WriteObjectEnd();
        }
        writer.WriteArrayEnd();
    }

    private void Floor2Json(JsonWriter writer, IGrouping<HouseEntityType, Transform> group)
    {
        writer.WritePropertyName("floor");
        writer.WriteArrayStart();
        foreach (var item in group)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName(item.name);
            writer.WriteObjectStart();
            writer.WritePropertyName("pos");
            var sa = item.GetComponent<MeshRenderer>().material.GetTextureScale("_BaseMap");
            writer.Write($"{item.position.x:f2},{item.position.z:f2},{item.localScale.x * 10:f2},{item.localScale.z * 10:f2},{sa.x:f2},{sa.y:f2}");
            writer.WritePropertyName("priority");
            int a = Mathf.CeilToInt(item.position.y * 100);
            writer.Write(a);
            writer.WritePropertyName("color");
            Color32 ma = item.GetComponent<MeshRenderer>().material.color;
            writer.Write($"{ma.r},{ma.g},{ma.b},{ma.a}");
            writer.WriteObjectEnd();
            writer.WriteObjectEnd();
        }
        writer.WriteArrayEnd();
    }
    private void Eye2Json(JsonWriter writer, IGrouping<HouseEntityType, Transform> group)
    {
        writer.WritePropertyName("look");
        writer.WriteArrayStart();
        foreach (var item in group)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName(item.name);
            writer.WriteObjectStart();
            writer.WritePropertyName("pos");
            var sa = item.GetComponent<Eye>();
            writer.Write($"{item.position.x:f2},{item.position.z:f2},{item.position.y:f2},{item.eulerAngles.x:f2},{item.eulerAngles.y:f2},{item.eulerAngles.z:f2},{item.localScale.x:f2},{item.localScale.y:f2},{item.localScale.z:f2}");
            writer.WritePropertyName("priority");
            writer.Write(sa.priority);
            writer.WritePropertyName("text");
            writer.Write(sa.textMesh.text);
            //writer.WritePropertyName("color");
            //Color32 ma = item.GetComponent<MeshRenderer>().material.color;
            //writer.Write($"{ma.r},{ma.g},{ma.b},{ma.a}");
            writer.WriteObjectEnd();
            writer.WriteObjectEnd();
        }
        writer.WriteArrayEnd();
    }
    private void Wall2Json(JsonWriter writer, IGrouping<HouseEntityType, Transform> group)
    {
        writer.WritePropertyName("wall");
        writer.WriteArrayStart();
        foreach (var item in group)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName(item.name);
            writer.WriteObjectStart();
            writer.WritePropertyName("pos");
            writer.Write($"{item.position.x:f2},{item.position.z:f2},{item.position.y:f2},{item.eulerAngles.y:f2},{item.localScale.x:f2},{item.localScale.y:f2},{item.localScale.z:f2}");
            writer.WritePropertyName("color");
            Color32 ma = item.GetComponent<MeshRenderer>().material.color;
            writer.Write($"{ma.r},{ma.g},{ma.b},{ma.a}");
            writer.WriteObjectEnd();
            writer.WriteObjectEnd();
        }
        writer.WriteArrayEnd();
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