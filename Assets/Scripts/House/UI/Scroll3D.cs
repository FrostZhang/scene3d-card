using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scroll3D : MonoBehaviour
{
    public ScrollRect scroll;
    public Transform biaochi;
    // Start is called before the first frame update
    void Start()
    {
        var ims = scroll.content.GetComponentsInChildren<Image3d>();
        scroll.onValueChanged.AddListener((x) =>
        {
            Vector2 bpos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(scroll.viewport, (biaochi as RectTransform).position, null, out bpos);
            foreach (var item in ims)
            {
                Vector2 local;
                var b = RectTransformUtility.ScreenPointToLocalPointInRectangle(scroll.viewport, item.rectTransform.position, null, out local);
                if (local.x <= bpos.x)
                {
                    var c = local.x - bpos.x;
                    item.deformation = new Vector4(0, 0, c * 0.06f, -c * 0.06f);
                    item.SetAllDirty();
                }
                else
                {
                    var c = local.x - bpos.x;
                    item.deformation = new Vector4(c * 0.06f, -c * 0.06f, 0, 0);
                    item.SetAllDirty();
                }
            }
        });
    }

}
