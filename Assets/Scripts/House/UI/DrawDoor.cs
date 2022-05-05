using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DrawDoor : MonoBehaviour
{
    public DragIcon prefab;
    List<DragIcon> fls;
    CreatPanel.Ziyuan choose;

    void Awake()
    {
        prefab.gameObject.SetActive(false);
    }
    void Start()
    {
        Ini();
    }

    async void Ini()
    {
        await new WaitUntil(() => CreatPanel.Instance.zdic != null);
        var dic = CreatPanel.Instance.zdic;
        if (dic.ContainsKey("door"))
        {
            var fs = dic["door"];
            fls = new List<DragIcon>(fs.Count);
            for (int i = 0; i < fs.Count; i++)
            {
                var f = fs[i];
                var te = await Help.Instance.TextureRequest(true, f.previewpath);
                if (!te) continue;
                var p = Instantiate(prefab, prefab.transform.parent);
                p.gameObject.SetActive(true);
                p.BeginDrag = () =>
                {
                    choose = f;
                    Reconstitution.Instance.CloseRTEditor();
                    CameraControllerForUnity.Instance.canUseMouseCenter = false;

                };
                p.EndDrag = (pointer) =>
                {
                    Reconstitution.Instance.OpenRTEditor();
                    CameraControllerForUnity.Instance.canUseMouseCenter = true;
                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        CreatAreaLight();
                    }
                    choose = null;
                };
                (p.targetGraphic as RawImage).texture = te;
                fls.Add(p);
            }
        }
    }

    //void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        if (!draw || EventSystem.current.IsPointerOverGameObject()) return;
    //        oldpos = Input.mousePosition;
    //        candraw = true;
    //    }
    //    else if (Input.GetMouseButtonUp(0))
    //    {
    //        candraw = false;
    //        if (!draw || EventSystem.current.IsPointerOverGameObject()) return;
    //        CreatAreaLight();
    //    }
    //}
    //void OnRenderObject()
    //{
    //    if (candraw)
    //    {
    //        GL.PushMatrix();
    //        discMaterial.SetPass(0);
    //        GL.LoadOrtho();
    //        GL.Begin(GL.LINE_STRIP);
    //        var v1 = new Vector3(oldpos.x / Screen.width, oldpos.y / Screen.height, oldpos.z);
    //        var v2 = new Vector3(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height, Input.mousePosition.z);
    //        GL.Vertex(v1);
    //        GL.Vertex(new Vector3(v1.x, v2.y, v2.z));
    //        GL.Vertex(v2);
    //        GL.Vertex(new Vector3(v2.x, v1.y, v2.z));
    //        GL.Vertex(v1);
    //        GL.End();
    //        GL.PopMatrix();
    //    }

    //}
    private async void CreatAreaLight()
    {
        var p = Input.mousePosition;
        p = Camera.main.ScreenToWorldPoint(p);
        var wall = await House.Instance.CreatDoor(choose.key
                             , $"{p.x},{p.z},1,0,90"
                             , null
                             , null
                             );
        await new WaitForEndOfFrame();
        wall.ReconstitutionMode(true);
        RTEditor.EditorObjectSelection.Instance.FixedSelectObj(wall.transform, RTEditor.GizmoType.VolumeScale);
    }
}
