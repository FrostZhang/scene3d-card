using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eye : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public Collider cd;
    public TextMesh textMesh;
    public int priority;
    void Awake()
    {
        Show(false);
    }

    public async void OnMouseUpAsButton()
    {
        if (!isEdit)
        {
            CameraControllerForUnity.Instance.enabled = false;
            if (CameraControllerForUnity.Instance.mode != CameraControllerForUnity.Mode.first)
            {
                var tr = CameraControllerForUnity.Instance.CaTr;
                var x = CameraControllerForUnity.Instance.xAngle;
                var y = CameraControllerForUnity.Instance.yAngle;
                CameraControllerForUnity.Instance.Firstfocus(tr.position, new Vector2(y, x));
            }
            float t = 0;
            var endpos = transform.position;
            var startpos = CameraControllerForUnity.Instance.transform.position;
            while ((t += Time.deltaTime) < 1)
            {
                var pos = Vector3.Lerp(startpos, endpos, t);
                var rot = Quaternion.LookRotation(pos - CameraControllerForUnity.Instance.transform.position);
                CameraControllerForUnity.Instance.transform.rotation = Quaternion.RotateTowards(CameraControllerForUnity.Instance.transform.rotation, rot, 2);
                CameraControllerForUnity.Instance.transform.position = pos;
                await new WaitForEndOfFrame();
            }
            CameraControllerForUnity.Instance.transform.position = endpos;
            var angle = CameraControllerForUnity.Instance.transform.eulerAngles;
            CameraControllerForUnity.Instance.xAngle = angle.y;
            CameraControllerForUnity.Instance.yAngle = angle.x;
            CameraControllerForUnity.Instance.enabled = true;
        }
    }

    internal void Show(bool value)
    {
        if (value)
        {
            cd.enabled = true;
            meshRenderer.enabled = true;
            textMesh.gameObject.SetActive(true);
        }
        else
        {
            cd.enabled = false;
            meshRenderer.enabled = false;
            textMesh.gameObject.SetActive(false);
        }
    }
    bool isEdit;
    internal void ReconstitutionMode(bool v)
    {
        isEdit = v;
        Show(v);
    }
}
