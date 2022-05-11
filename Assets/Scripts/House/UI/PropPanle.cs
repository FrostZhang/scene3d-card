using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PropPanle : MonoBehaviour
{
    public static PropPanle Instance;
    public Transform v1p;
    public Transform v2p;
    public Transform v3p;
    public Transform cp;
    public ScrollRect scroll;
    public Button delete;
    public PSColorPanel pSColor;
    List<Transform> trs;
    List<InputField> fs;
    public Action Ondel;
    void Start()
    {
        Instance = this;
        trs = new List<Transform>(10);
        fs = new List<InputField>(15);
        v1p.gameObject.SetActive(false);
        v2p.gameObject.SetActive(false);
        v3p.gameObject.SetActive(false);
        cp.gameObject.SetActive(false);
        gameObject.SetActive(false);
        delete.onClick.AddListener(() =>
        {
            Ondel?.Invoke();
        });
    }

    void OnEnable()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLInput.captureAllKeyboardInput = true;
#endif
    }

    void OnDisable()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLInput.captureAllKeyboardInput = false;
#endif
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            var g = EventSystem.current.currentSelectedGameObject;
            var ff = g.GetComponent<InputField>();
            if (ff)
            {
                var index = fs.FindIndex(x => x == ff);
                if (index == fs.Count - 1)
                {
                    fs[0].Select();
                }
                else
                {
                    fs[index + 1].Select();
                }
                //var h1 = scroll.content.rect.height;
                //var h2 = scroll.viewport.rect.height;
                //scroll.verticalNormalizedPosition = scroll.viewport.rect.height
            }
        }
    }

    public void GetV1(string title, float df, Action<float> act)
    {
        var v = Instantiate(v1p, v1p.transform.parent);
        v.GetComponent<Text>().text = title;
        var ff = v.GetComponentInChildren<InputField>();
        ff.onValueChanged.AddListener((x) =>
        {
            if (!string.IsNullOrWhiteSpace(x))
            {
                float.TryParse(x, out df);
                act.Invoke(df);
            }
        });
        fs.Add(ff);
        ff.text = df.ToString("f2");
        v.gameObject.SetActive(true);
        trs.Add(v);
    }

    public void GetEntity(string title, string df, Action<string> act)
    {
        var v = Instantiate(v1p, v1p.transform.parent);
        v.GetComponent<Text>().text = title;
        var ff = v.GetComponentInChildren<InputField>();
        ff.contentType = InputField.ContentType.Standard;
        ff.onValueChanged.AddListener((x) =>
        {
            if (!string.IsNullOrWhiteSpace(x))
            {
                act.Invoke(x);
            }
        });
        fs.Add(ff);
        ff.text = df;
        v.gameObject.SetActive(true);
        trs.Add(v);
    }

    public void GetV2(string title, float df, float df2, Action<float> act1, Action<float> act2)
    {
        var v = Instantiate(v2p, v2p.transform.parent);
        v.GetComponent<Text>().text = title;
        var ff = v.GetChild(0).GetComponent<InputField>();
        ff.onValueChanged.AddListener((x) =>
        {
            if (!string.IsNullOrWhiteSpace(x))
            {
                float.TryParse(x, out df);
                act1.Invoke(df);
            }
        });
        fs.Add(ff);
        ff.text = df.ToString("f2");
        ff = v.GetChild(1).GetComponent<InputField>();
        ff.onValueChanged.AddListener((x) =>
        {
            if (!string.IsNullOrWhiteSpace(x))
            {
                float.TryParse(x, out df2);
                act2.Invoke(df2);
            }
        });
        fs.Add(ff);
        ff.text = df2.ToString("f2");
        v.gameObject.SetActive(true);
        trs.Add(v);
    }

    public void GetV3(string title, float df, float df2, float df3, Action<float> act1, Action<float> act2, Action<float> act3)
    {
        var v = Instantiate(v3p, v3p.transform.parent);
        v.GetComponent<Text>().text = title;
        var ff = v.GetChild(0).GetComponent<InputField>();
        ff.onValueChanged.AddListener((x) =>
        {
            if (!string.IsNullOrWhiteSpace(x))
            {
                float.TryParse(x, out df);
                act1.Invoke(df);
            }
        });
        fs.Add(ff);
        ff.text = df.ToString("f2");
        ff = v.GetChild(1).GetComponent<InputField>();
        ff.onValueChanged.AddListener((x) =>
        {
            if (!string.IsNullOrWhiteSpace(x))
            {
                float.TryParse(x, out df2);
                act2.Invoke(df2);
            }
        });
        fs.Add(ff);
        ff.text = df2.ToString("f2");
        ff = v.GetChild(2).GetComponent<InputField>();
        ff.onValueChanged.AddListener((x) =>
        {
            if (!string.IsNullOrWhiteSpace(x))
            {
                float.TryParse(x, out df3);
                act3.Invoke(df3);
            }
        });
        fs.Add(ff);
        ff.text = df3.ToString("f2");
        v.gameObject.SetActive(true);
        trs.Add(v);
    }

    public void GetColor(string title, Color32 c, Action<Color> actc)
    {
        var v = Instantiate(cp, cp.transform.parent);
        v.gameObject.SetActive(true);
        var b = v.GetComponentInChildren<Button>();
        v.GetComponentInChildren<Text>().text = title;
        b.onClick.AddListener(() =>
        {
            if (pSColor.OnColorChanged != null)
            {
                pSColor.gameObject.SetActive(false);
                pSColor.OnColorChanged = null;
                return;
            }
            pSColor.gameObject.SetActive(true);
            pSColor.OnColorChanged = (x) =>
            {
                actc?.Invoke(x);
                b.targetGraphic.color = x;
            };
            pSColor.SetColor(b.targetGraphic.color);
        });
        b.targetGraphic.color = c;
        trs.Add(v);
    }

    public PropPanle Clear()
    {
        foreach (var item in trs)
        {
            Destroy(item.gameObject);
        }
        trs.Clear();
        fs.Clear();
        pSColor.gameObject.SetActive(false);
        pSColor.OnColorChanged = null;
        return this;
    }

    public void Show(bool b)
    {
        gameObject.SetActive(b);
        pSColor.gameObject.SetActive(false);
        pSColor.OnColorChanged = null;
    }

    public void Flush(int[] index, params float[] values)
    {
        for (int i = 0; i < index.Length; i++)
        {
            fs[index[i]].SetTextWithoutNotify(values[i].ToString("f2"));
        }
    }
}
