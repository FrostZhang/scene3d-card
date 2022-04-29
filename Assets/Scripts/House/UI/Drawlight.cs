using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Drawlight : MonoBehaviour
{
    public ToggleGroup tg;
    Toggle[] tgs;

    bool draw;
    Color32 lightC;
    private Vector3 oldpos;
    private bool candraw;
    Material discMaterial;
    void Awake()
    {
        tgs = tg.GetComponentsInChildren<Toggle>();

        foreach (var item in tgs)
        {
            item.onValueChanged.AddListener(OnSelectChange);
        }

        discMaterial = RTEditor.MaterialPool.Instance.GizmoSolidComponent;
        discMaterial.SetInt("_ZTest", 0);
        discMaterial.SetInt("_ZWrite", 1);
        discMaterial.SetInt("_IsLit", 0);
        discMaterial.SetInt("_CullMode", 0);
    }

    private void OnSelectChange(bool b)
    {
        if (b)
        {
            foreach (var item in tgs)
            {
                if (item.isOn)
                {
                    lightC = item.targetGraphic.color;
                    Reconstitution.Instance.CloseRTEditor();
                    CameraControllerForUnity.Instance.canUseMouseCenter = false;
                    draw = true;
                    break;
                }
            }
        }
        else
        {
            Reconstitution.Instance.OpenRTEditor();
            CameraControllerForUnity.Instance.canUseMouseCenter = true;
            draw = false;
            candraw = false;
        }
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!draw || EventSystem.current.IsPointerOverGameObject()) return;
            oldpos = Input.mousePosition;
            candraw = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            candraw = false;
            if (!draw || EventSystem.current.IsPointerOverGameObject()) return;
            CreatAreaLight();
        }
    }

    public async void CreatAreaLight()
    {
        var p = Input.mousePosition;
        if (Mathf.Abs(p.x - oldpos.x) < 10 || Mathf.Abs(p.y - oldpos.y) < 10) return;
        oldpos = Camera.main.ScreenToWorldPoint(oldpos);
        p = Camera.main.ScreenToWorldPoint(p);
        var size = p - oldpos;
        var center = oldpos + size * 0.5f;
        var light = House.Instance.CreatAreaLight($"{center.x},{center.z},{Mathf.Abs(size.x)},{Mathf.Abs(size.z)},3"
            , null
            , $"{lightC.r},{lightC.g},{lightC.b},{lightC.a}"
            );
        await new WaitForEndOfFrame();
        light.ReconstitutionMode(true);
        foreach (var item in tgs)
        {
            item.isOn = false;
        }
        RTEditor.EditorObjectSelection.Instance.FixedSelectObj(light.transform, RTEditor.GizmoType.VolumeScale);
    }

    void OnRenderObject()
    {
        if (candraw)
        {
            GL.PushMatrix();
            discMaterial.SetPass(0);
            GL.LoadOrtho();
            GL.Begin(GL.LINE_STRIP);
            GL.Color(lightC);
            var v1 = new Vector3(oldpos.x / Screen.width, oldpos.y / Screen.height, oldpos.z);
            var v2 = new Vector3(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height, Input.mousePosition.z);
            GL.Vertex(v1);
            GL.Vertex(new Vector3(v1.x, v2.y, v2.z));
            GL.Vertex(v2);
            GL.Vertex(new Vector3(v2.x, v1.y, v2.z));
            GL.Vertex(v1);
            GL.End();
            GL.PopMatrix();
        }

    }
}
