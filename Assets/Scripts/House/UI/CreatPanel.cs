using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreatPanel : MonoBehaviour
{
    public static CreatPanel Instance;
    public Image icon;

    public class Ziyuan
    {
        public string root;
        public string key;
        public string previewpath;
    }
    List<Ziyuan> ziyuans;
    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    async void Start()
    {
        icon.gameObject.SetActive(false);
        var te = await Help.Instance.TextRequest(true, "3dscene.json");
        if (te != null)
        {
            ziyuans = new List<Ziyuan>(100);
            var jd = LitJson.JsonMapper.ToObject(te);
            if (jd.IsArray)
            {
                jd.Foreach((cusname, jd) =>
                {
                    var dic = jd as IDictionary;
                    if (dic.Contains("datapath"))
                    {
                        if (dic.Contains("previewpath"))
                        {

                        }
                    }

                });
            }
        }
    }

}
