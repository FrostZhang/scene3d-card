using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsPanel : MonoBehaviour
{
    public static BoundsPanel Instance;
    public MoveImage left, right, top, buttom;
    Transform target;
    Collider targetC;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        gameObject.SetActive(false);
        left.OnMove = LeftMove;
        right.OnMove = RightMove;
        top.OnMove = TopMove;
        buttom.OnMove = ButtomMove;
    }

    private void ButtomMove(float delta)
    {
        if (targetC)
        {
            var p = target.position;
            p.z = p.z + delta * 0.5f;
            var zl = ((targetC.bounds.size.z + delta) / targetC.bounds.size.z) - 1;
            var zl2 = target.rotation * (Vector3.forward * zl);
            var ss = target.localScale;
            ss.x -= ss.x * zl2.x;
            ss.y -= ss.y * zl2.y;
            ss.z -= ss.z * zl2.z;
            target.localScale = ss;
            target.position = p;
            FlushVer(targetC.bounds);
        }
    }

    private void TopMove(float delta)
    {
        if (targetC)
        {
            var p = target.position;
            p.z = p.z + delta * 0.5f;
            var zl = 1 - ((targetC.bounds.size.z + delta) / targetC.bounds.size.z);
            var zl2 = target.rotation * (Vector3.forward * zl);
            var ss = target.localScale;
            ss.x -= ss.x * zl2.x;
            ss.y -= ss.y * zl2.y;
            ss.z -= ss.z * zl2.z;
            target.localScale = ss;
            target.position = p;
            FlushVer(targetC.bounds);
        }
    }

    private void RightMove(float delta)
    {
        if (targetC)
        {
            var p = target.position;
            p.x = p.x + delta * 0.5f;
            var zl = 1 - ((targetC.bounds.size.x + delta) / targetC.bounds.size.x);
            var zl2 = target.rotation * (Vector3.left * zl);
            var ss = target.localScale;
            ss.x += ss.x * zl2.x;
            ss.y += ss.y * zl2.y;
            ss.z += ss.z * zl2.z;
            target.localScale = ss;
            target.position = p;
            FlushHor(targetC.bounds);
        }
    }

    private void LeftMove(float delta)
    {
        if (targetC)
        {
            var p = target.position;
            p.x = p.x + delta * 0.5f;
            var zl = ((targetC.bounds.size.x + delta) / targetC.bounds.size.x) - 1;
            var zl2 = target.rotation * (Vector3.left * zl);
            Debug.Log(target.rotation);
            var ss = target.localScale;
            ss.x += ss.x * zl2.x;
            ss.y += ss.y * zl2.y;
            ss.z += ss.z * zl2.z;
            target.localScale = ss;
            target.position = p;
            FlushHor(targetC.bounds);
        }
    }

    void FlushHor(Bounds b)
    {
        var a = b.center.z + b.extents.z;
        top.SetSize(new Vector3(b.center.x, 10, a), b.size.x);
        a = b.center.z - b.extents.z;
        buttom.SetSize(new Vector3(b.center.x, 10, a), b.size.x);
    }

    void FlushVer(Bounds b)
    {
        var a = b.center.x - b.extents.x;
        left.SetSize(new Vector3(a, 10, b.center.z), b.size.z);
        a = b.center.x + b.extents.x;
        right.SetSize(new Vector3(a, 10, b.center.z), b.size.z);
    }

    public void FlushAll(Bounds b)
    {
        var a = b.center.x - b.extents.x;
        left.SetSize(new Vector3(a, 10, b.center.z), b.size.z);
        a = b.center.x + b.extents.x;
        right.SetSize(new Vector3(a, 10, b.center.z), b.size.z);
        a = b.center.z + b.extents.z;
        top.SetSize(new Vector3(b.center.x, 10, a), b.size.x);
        a = b.center.z - b.extents.z;
        buttom.SetSize(new Vector3(b.center.x, 10, a), b.size.x);
    }

    public void Bingding(Transform t, Collider collider)
    {
        target = t;
        targetC = collider;
        gameObject.SetActive(targetC);
        if (targetC)
        {
            FlushAll(collider.bounds);
        }
    }
}
