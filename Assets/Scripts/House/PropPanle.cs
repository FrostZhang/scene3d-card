using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropPanle : MonoBehaviour
{
    public Transform v1p;
    public Transform v2p;
    public Transform v3p;
    List<Transform> trs;
    public static PropPanle Instance;
    void Start()
    {
        Instance = this;
        trs = new List<Transform>(10);
        v1p.gameObject.SetActive(false);
        v2p.gameObject.SetActive(false);
        v3p.gameObject.SetActive(false);
        gameObject.SetActive(false);
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
                act.Invoke(float.Parse(x));
            }
        });
        ff.text = df.ToString("f2");
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
                act1.Invoke(float.Parse(x));
            }
        });
        ff.text = df.ToString("f2");
        ff = v.GetChild(1).GetComponent<InputField>();
        ff.onValueChanged.AddListener((x) =>
        {
            if (!string.IsNullOrWhiteSpace(x))
            {
                act2.Invoke(float.Parse(x));
            }
        });
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
                act1.Invoke(float.Parse(x));
            }
        });
        ff.text = df.ToString("f2");
        ff = v.GetChild(1).GetComponent<InputField>();
        ff.onValueChanged.AddListener((x) =>
        {
            if (!string.IsNullOrWhiteSpace(x))
            {
                act2.Invoke(float.Parse(x));
            }
        });
        ff.text = df2.ToString("f2");
        ff = v.GetChild(2).GetComponent<InputField>();
        ff.onValueChanged.AddListener((x) =>
        {
            if (!string.IsNullOrWhiteSpace(x))
            {
                act3.Invoke(float.Parse(x));
            }
        });
        ff.text = df3.ToString("f2");
        v.gameObject.SetActive(true);
        trs.Add(v);
    }


    public PropPanle Clear()
    {
        foreach (var item in trs)
        {
            Destroy(item.gameObject);
        }
        trs.Clear();
        return this;
    }
}
