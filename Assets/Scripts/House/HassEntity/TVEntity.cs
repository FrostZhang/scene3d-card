using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVEntity : HassEntity
{
    public GameObject onOnj, high;
    public TextMesh te;
    string[] tes;
    void Start()
    {
        tes = new string[te.text.Length];
        string str = string.Empty;
        for (int i = 0; i < te.text.Length; i++)
        {
            str += te.text[i];
            tes[i] = str.Clone() as string;
        }
        onOnj.SetActive(false);
        GetComponent<BoxCollider>().enabled = true;
    }
    public override void MouseOn()
    {
        high.SetActive(true);
    }
    public override void MouseExit()
    {
        high.SetActive(false);
    }
    protected override void TrunOn()
    {
        onOnj.SetActive(true);
    }

    protected override void TrunOff()
    {
        onOnj.SetActive(false);
    }
    public override void ReconstitutionMode(bool enter)
    {
        //onOnj.SetActive(enter);
    }

    float interval = 3;
    float _interval;
    void Update()
    {
        if (onOnj.activeSelf)
        {
            if ((_interval -= Time.deltaTime) < 0)
            {
                _interval = interval;
                StartCoroutine(AnimText());
            }
        }
    }
    IEnumerator AnimText()
    {
        for (int i = 0; i < tes.Length; i++)
        {
            te.text = tes[i];
            yield return new WaitForSeconds(0.1f);
        }
    }
}
