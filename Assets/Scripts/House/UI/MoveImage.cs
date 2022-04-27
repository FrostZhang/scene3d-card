using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveImage : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool ishor;
    RectTransform rectTransform;
    Vector3 old;
    public Action<float> OnMove;
    void Awake()
    {
        rectTransform = transform as RectTransform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //old = eventData.position;
        var pos = eventData.position;
        old = Camera.main.ScreenToWorldPoint(eventData.position);
        CameraControllerForUnity.Instance.canUseMouseCenter = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //var delta = eventData.position - old;
        Vector3 pos = Camera.main.ScreenToWorldPoint(eventData.position);
        var delta = pos - old;
        old = pos;
        if (ishor)
        {
            OnMove?.Invoke(delta.z);
            var p = rectTransform.position;
            p.z += delta.z;
            rectTransform.position = p;
        }
        else
        {
            OnMove?.Invoke(delta.x);
            var p = rectTransform.position;
            p.x += delta.x;
            rectTransform.position = p;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        CameraControllerForUnity.Instance.canUseMouseCenter = true;
    }

    public void SetSize(Vector3 pos, float size)
    {
        rectTransform.position = pos;
        if (ishor)
        {
            var s = rectTransform.sizeDelta;
            s.x = size;
            rectTransform.sizeDelta = s;
        }
        else
        {
            var s = rectTransform.sizeDelta;
            s.y = size;
            rectTransform.sizeDelta = s;
        }
    }


}
