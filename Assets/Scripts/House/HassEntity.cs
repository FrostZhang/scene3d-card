using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HassEntity : MonoBehaviour
{
    public string entity_id;
    const string switchkey = "switch";//on off
    const string lightkey = "light";  //on off
    const string climate = "climate"; // on off
    const string binary_sensor = "binary_sensor"; //on off
    const string media_player = "media_player"; //idle
    const string person = "person"; //not_home
    const string cover = "cover"; //���� closed closing open opening
    const string fan = "fan"; //on off
    const string on = "on";
    const string off = "off";

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

    public virtual void Click()
    {
        if (string.IsNullOrEmpty(entity_id))
            return;
        HassMessage message = new HassMessage();
        message.head = head;
        message.entity_id = entity_id;
        if (open)
        {
            open = false;
            message.cmd = cmdoff;
        }
        else
        {
            open = true;
            message.cmd = cmdon;
        }
        Shijie.LightMessage(JsonUtility.ToJson(message));
    }

    public virtual void LongClick()
    {
        if (string.IsNullOrEmpty(entity_id))
            return;
    }
}