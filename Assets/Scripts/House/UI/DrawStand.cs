using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DrawStand : MonoBehaviour
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
        if (dic.ContainsKey("stand"))
        {
            var fs = dic["stand"];
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
                    if (!EventSystem.current.IsPointerOverGameObject())
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

    private async void CreatAreaLight()
    {
        var p = Input.mousePosition;
        p = Camera.main.ScreenToWorldPoint(p);
        Transform wall = null;
        wall = await House.Instance.CreatStand(choose.key, null);
        p.y = 0;
        wall.transform.position = p;
        await new WaitForEndOfFrame();
        wall.GetComponent<Collider>().enabled = true;
        Reconstitution.Instance.EditorTransform(wall);
    }
}
