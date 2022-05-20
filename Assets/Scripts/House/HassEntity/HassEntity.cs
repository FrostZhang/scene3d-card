using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HassEntity : MonoBehaviour
{
    public string Entity_id { get => entity_id; }
    public Collider editC;
    protected string entity_id;
    //const string switchkey = "switch";//on off
    //const string lightkey = "light";  //on off
    //const string climate = "climate"; // on off
    //const string binary_sensor = "binary_sensor"; //on off
    //const string media_player = "media_player"; //idle
    //const string person = "person"; //not_home
    //const string cover = "cover"; //���� closed closing open opening
    //const string fan = "fan"; //on off
    const string on = "on";
    const string off = "off";
    const string undefine = "undefine";

    string head;
    const string cmdon = "turn_on";
    const string cmdoff = "turn_off";
    protected bool open = false;

    public void SetEntity(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            head = null;
            entity_id = null;
            return;
        }
        var ids = id.Split('.');
        if (ids.Length == 2)
        {
            head = ids[0];
            entity_id = id;
        }
    }

    public void Click()
    {
        if (string.IsNullOrEmpty(entity_id))
        {
            if (open)
            {
                open = false;
                TrunOff();
            }
            else
            {
                open = true;
                TrunOn();
            }
            return;
        }
        HassServerMessage message = new HassServerMessage();
        message.head = head;
        message.entity_id = entity_id;
        if (open)
        {
            open = false;
            message.cmd = cmdoff;
            TrunOff();
        }
        else
        {
            open = true;
            message.cmd = cmdon;
            TrunOn();
        }
        Shijie.ClickMessage(JsonUtility.ToJson(message));
    }

    public virtual void MouseOn()
    {

    }

    /// <summary>hass ��Ϣ  ��</summary>
    protected virtual void TrunOn()
    {

    }
    /// <summary>hass ��Ϣ  ��</summary>
    protected virtual void TrunOff()
    {

    }
    /// <summary>hass ��Ϣ  ��on/off</summary>
    protected virtual void Destine(string state)
    {

    }

    protected virtual void AfterLongClick()
    {

    }

    public void StateMeassge(string state)
    {
        if (state == on)
        {
            if (!open)
            {
                open = true;
                TrunOn();
            }
        }
        else if (state == off)
        {
            if (open)
            {
                open = false;
                TrunOff();
            }
        }
        else
        {
            Destine(state);
        }
    }

    public virtual void LongClick()
    {
        if (!string.IsNullOrEmpty(entity_id))
        {
            HassMoreInfo info = new HassMoreInfo();
            info.entity_id = entity_id;
            Shijie.LongClickMessage(JsonUtility.ToJson(info));
        }
        AfterLongClick();
    }

    public virtual void MouseExit()
    {

    }

    /// <summary>�༭ģʽ </summary>
    public virtual void ReconstitutionMode(bool enter)
    {
        if (!editC)
            editC = GetComponent<Collider>();
        if (editC)
            editC.enabled = enter;
    }
}