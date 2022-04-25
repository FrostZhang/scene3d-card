using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorEntity : HassEntity
{
    Coroutine coroutine;
    public float angleclose, angleopen;

    protected virtual void Start()
    {
        GetComponent<BoxCollider>().enabled = false;
        var e = transform.eulerAngles;
        e.y = angleclose;
        transform.eulerAngles = e;
        TrunOff();
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
        var e = transform.eulerAngles;
        while (true)
        {
            var a = Mathf.LerpAngle(e.y, f, l += t * Time.deltaTime);
            transform.eulerAngles = new Vector3(e.x, a, e.z);
            yield return new WaitForEndOfFrame();
            if (l > 1)
                break;
        }
    }
}
