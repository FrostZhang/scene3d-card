using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HassEntity : MonoBehaviour
{
    public string Entity_id { get => entity_id; }
    protected string entity_id;
    //const string switchkey = "switch";//on off
    //const string lightkey = "light";  //on off
    //const string climate = "climate"; // on off
    //const string binary_sensor = "binary_sensor"; //on off
    //const string media_player = "media_player"; //idle
    //const string person = "person"; //not_home
    //const string cover = "cover"; //´°Á± closed closing open opening
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
            return;
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

    protected virtual void TrunOn()
    {

    }

    protected virtual void TrunOff()
    {

    }

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

    /// <summary>±à¼­Ä£Ê½ </summary>
    public virtual void ReconstitutionMode(bool enter)
    {
        var c = GetComponent<Collider>();
        if (c) c.enabled = enter;
    }
}
