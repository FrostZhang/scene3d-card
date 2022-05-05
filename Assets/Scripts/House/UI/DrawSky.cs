using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DrawSky : MonoBehaviour
{
    public Button prefab;
    List<Button> fls;

    void Awake()
    {
        prefab.gameObject.SetActive(false);
    }
    void Start()
    {
        Ini();
    }

    async void Ini()
    {
        await new WaitUntil(() => CreatPanel.Instance.zdic != null);
        var dic = CreatPanel.Instance.zdic;
        if (dic.ContainsKey("sky"))
        {
            var fs = dic["sky"];
            fls = new List<Button>(fs.Count);
            for (int i = 0; i < fs.Count; i++)
            {
                var f = fs[i];
                var te = await Help.Instance.TextureRequest(true, f.previewpath);
                if (!te) continue;
                var p = Instantiate(prefab, prefab.transform.parent);
                p.gameObject.SetActive(true);
                p.onClick.AddListener(() =>
                {
                    HouseWeather.Instance.CreatSky(f.key);
                });
                (p.targetGraphic as RawImage).texture = te;
                fls.Add(p);
            }
        }
    }

}
