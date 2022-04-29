using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragIcon : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
{
    public Image image;
    Image icon;
    public void OnBeginDrag(PointerEventData eventData)
    {
        icon = CreatPanel.Instance.icon;
        icon.sprite = image.sprite;
        icon.gameObject.SetActive(true);
        CameraControllerForUnity.Instance.canUseMouseCenter = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        icon.rectTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        icon.gameObject.SetActive(false);
        CameraControllerForUnity.Instance.canUseMouseCenter = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

}
