using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunEntity : HassEntity
{
    public Transform fan;
    Coroutine coroutine;
    public GameObject  high;
    void Awake()
    {
        high.SetActive(false);
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
        if (coroutine == null)
        {
            coroutine = StartCoroutine(Run());
        }
    }

    protected override void TrunOff()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    public override void ReconstitutionMode(bool enter)
    {
        if (enter)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }
        else
        {
            if (open)
            {
                coroutine = StartCoroutine(Run());
            }
        }
    }

    IEnumerator Run()
    {
        while (true)
        {
            var e = fan.eulerAngles;
            e.y += 1;
            if (e.y > 360)
            {
                e.y -= 360;
            }
            fan.eulerAngles = e;
            yield return null;
        }
    }
}