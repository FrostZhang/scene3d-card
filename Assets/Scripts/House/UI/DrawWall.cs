using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DrawWall : MonoBehaviour
{
    public Toggle prefab;
    List<Toggle> fls;
    private bool draw;
    CreatPanel.Ziyuan choose;
    private Vector3 oldpos;
    private bool candraw;
    Material discMaterial;
    void Awake()
    {
        discMaterial = RTEditor.MaterialPool.Instance.GizmoSolidComponent;
        discMaterial.SetInt("_ZTest", 0);
        discMaterial.SetInt("_ZWrite", 1);
        discMaterial.SetInt("_IsLit", 0);
        discMaterial.SetInt("_CullMode", 0);
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
        if (dic.ContainsKey("wall"))
        {
            var fs = dic["wall"];
            fls = new List<Toggle>(fs.Count);
            for (int i = 0; i < fs.Count; i++)
            {
                var f = fs[i];
                var te = await Help.Instance.TextureRequest(true, f.previewpath);
                if (!te) continue;
                var p = Instantiate(prefab, prefab.transform.parent);
                p.gameObject.SetActive(true);
                p.onValueChanged.AddListener((x) =>
                {
                    if (x)
                    {
                        choose = f;
                        draw = true;
                        Reconstitution.Instance.CloseRTEditor();
                        CameraControllerForUnity.Instance.canUseMouseCenter = false;
                    }
                    else
                    {
                        draw = false;
                        choose = null;
                        Reconstitution.Instance.OpenRTEditor();
                        CameraControllerForUnity.Instance.canUseMouseCenter = true;
                    }
                });
                (p.targetGraphic as RawImage).texture = te;
                fls.Add(p);
            }
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
    void OnRenderObject()
    {
        if (candraw)
        {
            GL.PushMatrix();
            discMaterial.SetPass(0);
            GL.LoadOrtho();
            GL.Begin(GL.LINE_STRIP);
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
    private async void CreatAreaLight()
    {
        var p = Input.mousePosition;
        if (Mathf.Abs(p.x - oldpos.x) < 10 || Mathf.Abs(p.y - oldpos.y) < 10) return;
        oldpos = Camera.main.ScreenToWorldPoint(oldpos);
        p = Camera.main.ScreenToWorldPoint(p);
        var size = p - oldpos;
        var center = oldpos + size * 0.5f;
        Transform wall = null;
        if (Mathf.Abs(size.x) > Mathf.Abs(size.z))
        {
            wall = await House.Instance.CreatWall(choose.key
                                , $"{center.x},{oldpos.z},{Mathf.Abs(size.x)},1,0.125,0"
                                , "195,216,235,255"
                                );
        }
        else
        {
            wall = await House.Instance.CreatWall(choose.key
                    , $"{oldpos.x},{center.z},{Mathf.Abs(size.z)},1,0.125,90"
                    , "195,216,235,255"
                    );
        }
        await new WaitForEndOfFrame();
        wall.GetComponent<Collider>().enabled = true;
        foreach (var item in fls)
            item.isOn = false;
        RTEditor.EditorObjectSelection.Instance.FixedSelectObj(wall, RTEditor.GizmoType.VolumeScale);
    }
}
