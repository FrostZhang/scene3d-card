using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    float lastdown;
    Vector3 lastv3;
    void OnMouseDown()
    {
        lastdown = Time.time;
        lastv3 = Input.mousePosition;
    }

    Coroutine coroutine;
    void OnMouseUp()
    {
        if (Time.time - lastdown > 0.5f || Vector3.Magnitude(Input.mousePosition - lastv3) > 16f)
            return;
        if (Time.time - lastdown < 0.5f)
        {
            StopAllCoroutines();
            HassMessage message = new HassMessage();
            message.head = SWITCH;
            message.entity_id = entity_id;
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
                Shijie.LightMessage(JsonUtility.ToJson(message));
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