using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZebraEntity : HassEntity
{
    public static int info = Animator.StringToHash("AnimInfo");
    public Animator anim;
    public Transform Area { get => area;}

    Transform area;


    public void SetArea(Transform plane)
    {
        area = plane;
        transform.position = plane.position;
    }

    void Start()
    {
        NextAnim();
    }

    private void NextAnim()
    {
        if (!transform) return;
        if (Random.value > 0.4f)
        {
            Vector3 pos = area.TransformPoint(new Vector3(Random.value - 0.5f, 0, Random.value - 0.5f));
            StartCoroutine(WalkTo(pos));
        }
        else
        {
            StartCoroutine(Stand());
        }
    }

    IEnumerator Stand()
    {
        anim.SetInteger(info, 0);
        yield return new WaitForSeconds(Random.value * 10);
        NextAnim();
    }

    IEnumerator WalkTo(Vector3 pos)
    {
        var p = transform.position;
        float t = 0;
        var speed = Vector3.Distance(pos, p);
        speed = 1 / speed;
        anim.SetInteger(info, 1);
        transform.LookAt(pos);
        while (transform)
        {
            transform.position = Vector3.Lerp(p, pos, t += Time.deltaTime * speed);
            if (t > 1)
                break;
            yield return new WaitForEndOfFrame();
        }
        NextAnim();
    }
}
