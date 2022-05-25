using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEntity : HassEntity
{
    Coroutine coroutine;

    public Light clight;
    float max;
    Renderer _renderer;

    public float Max { get => max; set => max = value; }

    void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        _renderer.enabled = false;
        max = clight.intensity;
        clight.range = max;
        clight.intensity = 0;
    }

    public override void MouseExit()
    {
        _renderer.enabled = false;
    }

    public override void MouseOn()
    {
        _renderer.enabled = true;
    }

    protected override void TrunOn()
    {
        StopAllCoroutines();
        coroutine = StartCoroutine(To(max));
    }

    protected override void TrunOff()
    {
        StopAllCoroutines();
        coroutine = StartCoroutine(To(0));
    }

    public override void ReconstitutionMode(bool enter)
    {
        _renderer.enabled = enter;
        //还原调试带来的灯光
        if (!enter)
        {
            if (open)
            {
                clight.intensity = max;
            }
            else
            {
                clight.intensity = 0;
            }
        }
    }

    IEnumerator To(float f)
    {
        float t = 1 / 3f;
        float l = 0;
        while (true)
        {
            clight.intensity = Mathf.Lerp(clight.intensity, f, l += t * Time.deltaTime);
            yield return new WaitForEndOfFrame();
            if (l > 1)
                break;
        }
    }
}
