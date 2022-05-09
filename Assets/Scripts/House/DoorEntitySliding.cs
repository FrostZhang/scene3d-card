using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorEntitySliding : DoorEntity
{
    Coroutine coroutine;
    public Transform yidong;
    protected override void Start()
    {
        GetComponent<BoxCollider>().enabled = false;
        angleopen = -0.11f;
        angleclose = -0.958f;
        TrunOff();
        var mrs = transform.GetComponentsInChildren<MeshRenderer>();
        var ma = transform.GetComponent<MeshRenderer>().material;
        foreach (var item in mrs)
        {
            item.material = ma;
        }
    }

    protected override void TrunOn()
    {
        StopAllCoroutines();
        coroutine = StartCoroutine(To(angleopen));
    }

    protected override void TrunOff()
    {
        StopAllCoroutines();
        coroutine = StartCoroutine(To(angleclose));
    }

    IEnumerator To(float f)
    {
        float t = 1 / 2f;
        float l = 0;
        var p = yidong.localPosition;
        var start = p;
        p.x = f;
        while (true)
        {
            yidong.localPosition = Vector3.Lerp(start, p, l += t * Time.deltaTime);
            yield return new WaitForEndOfFrame();
            if (l > 1)
                break;
        }
    }
}
