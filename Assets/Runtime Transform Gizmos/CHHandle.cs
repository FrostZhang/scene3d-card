using RTEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CHHandle
{
    List<TranslationGizmo> pool;
    List<VolumeScaleGizmo> volumepool;
    ScaleGizmo scaleGizmo;
    public Transform pa;
    public CHHandle()
    {
        pool = new List<TranslationGizmo>(10);
        volumepool = new List<VolumeScaleGizmo>(10);
        pa = new GameObject("Handles").transform;
    }

    public TranslationGizmo PositionHandle(Vector3 vector3, Quaternion quaternion, Action<Vector3> delta)
    {
        if (pool.Count > 0)
        {
            var p = pool[pool.Count - 1];
            pool.RemoveAt(pool.Count - 1);
            p.transform.position = vector3;
            p.transform.rotation = quaternion;
            p.gameObject.SetActive(true);
            p.OnMove = delta;
            return p;
        }
        var t = Resources.Load<TranslationGizmo>("Translation Gizmo");
        var gm = UnityEngine.Object.Instantiate(t, pa);
        gm.transform.position = vector3;
        gm.transform.rotation = quaternion;
        gm.OnMove = delta;
        return gm;
    }
    /// <summary>action: scale postion </summary>
    public VolumeScaleGizmo VolumeHandle(GameObject go, Action<Vector3, Vector3> delta)
    {
        if (volumepool.Count > 0)
        {
            var p = volumepool[volumepool.Count - 1];
            volumepool.RemoveAt(volumepool.Count - 1);
            p.gameObject.SetActive(true);
            p.ControlledObjects = new List<GameObject>() { go };
            p.OnMove = delta;
            return p;
        }
        var t = Resources.Load<VolumeScaleGizmo>("Volume Scale Gizmo");
        var gm = UnityEngine.Object.Instantiate(t, pa);
        gm.ControlledObjects = new List<GameObject>() { go };
        gm.OnMove = delta;
        return gm;
    }

    public ScaleGizmo ScaleHandle(Vector3 pos, Quaternion quaternion, Func<Vector3> begin, Action<Vector3> end)
    {
        if (!scaleGizmo)
        {
            var t = Resources.Load<ScaleGizmo>("Scale Gizmo");
            scaleGizmo = UnityEngine.Object.Instantiate(t, pa);
        }
        scaleGizmo.gameObject.SetActive(true);
        scaleGizmo.transform.position = pos;
        scaleGizmo.transform.rotation = quaternion;
        scaleGizmo.OnMove = end;
        scaleGizmo.OnBegin = begin;
        return scaleGizmo;
    }

    public void ReHandle(TranslationGizmo gizmo)
    {
        if (!pool.Contains(gizmo))
        {
            pool.Add(gizmo);
            gizmo.gameObject.SetActive(false);
        }
    }

    public void ReHandle(VolumeScaleGizmo gizmo)
    {
        if (!volumepool.Contains(gizmo))
        {
            volumepool.Add(gizmo);
            gizmo.gameObject.SetActive(false);
        }
    }

    public void ReHandle(ScaleGizmo gizmo)
    {
        gizmo.gameObject.SetActive(true);
    }

    public void ReHandle(Gizmo gizmo)
    {
        if (gizmo is TranslationGizmo)
        {
            ReHandle(gizmo as TranslationGizmo);
        }
        else if (gizmo is VolumeScaleGizmo)
        {
            ReHandle(gizmo as VolumeScaleGizmo);
        }
        else if (gizmo is ScaleGizmo)
        {
            ReHandle(gizmo as ScaleGizmo);
        }
    }
}
