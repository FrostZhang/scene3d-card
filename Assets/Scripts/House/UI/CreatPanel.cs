using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CreatPanel : MonoBehaviour
{
    public static CreatPanel Instance;
    public RawImage icon;
    public Transform btns, panels;
    public Dictionary<string, List<Ziyuan>> zdic { get; private set; }
    public class Ziyuan
    {
        public string root;
        public string key;
        public string previewpath;
        public override string ToString()
        {
            return root + " " + key + " " + previewpath;
        }
    }
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        AnsJson();
        icon.gameObject.SetActive(false);
        var bs = btns.GetComponentsInChildren<Button>();
        for (int i = 0; i < bs.Length; i++)
        {
            var b = bs[i];
            b.onClick.AddListener(() =>
            {
                foreach (Transform item in panels)
                {
                    if (item.name != b.name)
                    {
                        item.gameObject.SetActive(false);
                    }
                    else
                    {
                        item.gameObject.SetActive(true);
                    }
                }
            });
        }
        Show(false);
    }

    public void Show(bool b)
    {
        gameObject.SetActive(b);
    }

    private async Task AnsJson()
    {
        var te = await Help.Instance.TextRequest(true, "3dscene.json");
        if (te != null)
        {
            zdic = new Dictionary<string, List<Ziyuan>>(200);
            var jd = LitJson.JsonMapper.ToObject(te);
            if (jd.IsArray)
            {
                jd.Foreach((cusname, jd) =>
                {
                    var dic = jd as IDictionary;
                    if (dic.Contains("datapath"))
                    {
                        Ziyuan ziyuan = new Ziyuan();
                        var d = dic["datapath"]?.ToString();
                        if (!string.IsNullOrEmpty(d))
                        {
                            d = d.Substring(d.IndexOf('/') + 1);
                            d = d.Remove(d.IndexOf('.'));
                            var ds = d.Split('_');
                            ziyuan.key = ds[0];
                            ziyuan.root = ds[1];
                            if (zdic.ContainsKey(ziyuan.root))
                            {
                                zdic[ziyuan.root].Add(ziyuan);
                            }
                            else
                            {
                                zdic.Add(ziyuan.root, new List<Ziyuan>() { ziyuan });
                            }
                        }
                        if (dic.Contains("previewpath"))
                        {
                            var p = dic["previewpath"];
                            ziyuan.previewpath = p?.ToString();
                        }
                    }
                });
            }
        }
    }
}
