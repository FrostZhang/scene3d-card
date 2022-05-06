using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RawImage targetGraphic;
    RawImage icon;
    public Action BeginDrag;
    public Action<PointerEventData> EndDrag;

    public void OnBeginDrag(PointerEventData eventData)
    {
        icon = CreatPanel.Instance.icon;
        icon.texture = targetGraphic.texture;
        icon.gameObject.SetActive(true);
        BeginDrag?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        icon.rectTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        icon.gameObject.SetActive(false);
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        EndDrag?.Invoke(eventData);
    }

}
