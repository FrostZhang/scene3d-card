using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorHandle : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler
{
    public RectTransform yuanxin;
    public RectTransform handle;

    public Material show;
    public float dis = 292;
    Image handleIm;

    /// <summary>
    /// É«²Ê int res = 213 | 103 << 8 | 78 << 16;
    /// </summary>
    public Action<int> OnColorChange;
    private void Awake()
    {
        handleIm = handle.GetComponent<Image>();
        dis = Vector3.Distance(handle.position, yuanxin.position);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        Set(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Set(eventData, true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Set(eventData);
    }

    private void Set(PointerEventData eventData, bool needSend = false)
    {
        if (!Application.isPlaying)
        {
            return;
        }
        Vector2 pos = yuanxin.position;
        var dir = eventData.position - pos;
        var p = pos + (dir.normalized) * dis;
        float theta = Mathf.Acos(Vector2.Dot(dir.normalized, Vector2.right));
        if (dir.y < 0)
        {
            theta = 2 * Mathf.PI - theta;
        }
        var h = theta / 2 / Mathf.PI;

        Vector3 localPos = handle.localPosition;
        Vector2 t = new Vector2(localPos.x, localPos.y) / yuanxin.rect.size + new Vector2(0.5f, 0.5f);

        float sqrt3dv2 = 0.8660254037844386f;
        float oneminus = 1 - sqrt3dv2;

        var v = (Mathf.Clamp(t.y, oneminus, 1) - oneminus) / sqrt3dv2;
        float temp = v / 2;
        var s = Mathf.Clamp(t.x, 0.5f - temp, 0.5f + temp);
        s = v == 0 ? 0 : (s - 0.5f + temp) / v;

        Color color = Color.HSVToRGB(h, 1, 1);
        OnColorChange?.Invoke((int)(color.r * 255) | (int)(color.g * 255) << 8 | (int)(color.b * 255) << 16);
        handle.position = p;
        show.SetFloat("_Hue", h * 360);
    }

    const float RingLen = 0.27f;
    public void SetColor(float h)
    {
        float theta = h * Mathf.PI * 2;
        float x = Mathf.Cos(theta);
        float y = Mathf.Sin(theta);
        Vector2 dir = new Vector2(x, y);
        dir = (1 - RingLen * 0.5f) * dir.normalized;
        Vector2 pos = yuanxin.position;
        handle.position = pos + (dir.normalized) * dis;
        show.SetFloat("_Hue", h * 360);
    }

}
