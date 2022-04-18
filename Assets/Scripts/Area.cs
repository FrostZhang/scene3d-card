using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Area : MonoBehaviour
{
    const string SWITCH = "switch";
    const string TOGGLE = "toggle";
    const string TURN_OFF = "turn_off";
    const string TURN_ON = "turn_on";

    public Light clight;
    float max;
    bool open;
    public string entity_id;
    void Start()
    {
        max = clight.intensity;
        clight.intensity = 0;
    }

    public void SetHassEntity_id(string id)
    {
        this.entity_id = id;
    }

    float lastdown;
    Vector3 lastv3;
    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        if (EventSystem.current.IsPointerOverGameObject(0))
            return;
        if (EventSystem.current.IsPointerOverGameObject(1))
            return;
        lastdown = Time.time;
        lastv3 = Input.mousePosition;
    }

    Coroutine coroutine;
    void OnMouseUp()
    {
        if (Vector3.Magnitude(Input.mousePosition - lastv3) > 10f)
            return;
        if (Time.time - lastdown < 0.5f)
        {
            HassServerMessage message = new HassServerMessage();
            message.head = SWITCH;
            message.entity_id = entity_id;
            StopAllCoroutines();
            if (!open)
            {
                coroutine = StartCoroutine(To(max));
                open = true;
                message.cmd = TURN_ON;
            }
            else
            {
                open = false;
                coroutine = StartCoroutine(To(0));
                message.cmd = TURN_OFF;
            }
            if (!string.IsNullOrEmpty(entity_id))
            {
                Shijie.AsherLink3DClickMessage(JsonUtility.ToJson(message));
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(entity_id))
            {
                HassMoreInfo info = new HassMoreInfo();
                info.entity_id = entity_id;
                Shijie.AsherLink3DLongClickMessage(JsonUtility.ToJson(info));
            }
        }
    }

    IEnumerator To(float f)
    {
        float t = 1 / 3f;
        float l = 0;
        while (true)
        {
            clight.intensity = Mathf.Lerp(clight.intensity, f, l += t * Time.deltaTime);
            yield return new WaitForEndOfFrame();
            if (l>1)
                break;
        }
    }

    public void ChangeFromHass(string str)
    {
        bool b;
        if (str == "on")
        {
            b = true;
        }
        else if (str == "off")
        {
            b = false;
        }
        else
        {
            return;
        }
        if (open != b)
        {
            open = b;
            StopAllCoroutines();
            if (open)
            {
                coroutine = StartCoroutine(To(max));
            }
            else
            {
                coroutine = StartCoroutine(To(0));
            }
        }
    }
}