using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Reconstitution : MonoBehaviour
{
    public static Reconstitution Instance;
    public bool IsEdit { get => isEdit; }
    private bool isEdit;
    Camera mainCa;
    Transform caTr;
    Transform rigTr;
    Transform cs;
    private RaycastHit hit;
    private float maxDistance = 25;
    private Transform lasthit;
    Outline lastO;
    Vector3 lastmousePos, lastTrpos;
    bool drag;
    int intervel = 50;
    public Color lineColor = new Color(1, 1, 1, 0.5f);
    public Material lineMat;
    public LineRenderer lineW;
    public TextMesh linewTe;
    public LineRenderer lineH;
    public TextMesh linehTe;

    void Start()
    {
        Instance = this;
        mainCa = Camera.main;
        caTr = mainCa.transform;
        rigTr = CameraControllerForUnity.Instance.transform;
    }

    public void InEditToggle()
    {
        if (isEdit)
        {
            isEdit = false;
            CameraControllerForUnity.Instance.Thirdfocus(new Vector3(0, -1, 0), new Vector2(45, 0), 12);
            cs = GetComponent<House>().parent;
            foreach (Transform item in cs)
            {
                var e = item.GetComponent<HassEntity>();
                if (e)
                {
                    e.ReconstitutionMode(false);
                }
                else
                {
                    var c = item.GetComponent<Collider>();
                    if (c) c.enabled = false;
                }
            }
            return;
        }
        isEdit = true;
        rigTr.position = new Vector3(0, -1, 0);
        CameraControllerForUnity.Instance.MobaFollow_orthogonal(rigTr, new Vector2(90, 0), 15, 5);
        cs = GetComponent<House>().parent;
        foreach (Transform item in cs)
        {
            var e = item.GetComponent<HassEntity>();
            if (e)
            {
                e.ReconstitutionMode(true);
            }
            else
            {
                var c = item.GetComponent<Collider>();
                if (c) c.enabled = true;
            }
        }
    }

    void Update()
    {
        if (!isEdit)
            return;
        if (drag)
        {
            if (Input.GetMouseButtonUp(0))
            {
                drag = false;
                lineW.gameObject.SetActive(false);
                lineH.gameObject.SetActive(false);
                return;
            }
            var pos = mainCa.ScreenToWorldPoint(Input.mousePosition);
            pos.y = 0;
            pos = lastO.transform.position = lastTrpos + (pos - lastmousePos);
            var p1 = new Vector3(0, 10, pos.z);
            lineW.SetPosition(0, p1);
            lineW.SetPosition(1, new Vector3(pos.x, 10, pos.z));
            var p2 = new Vector3(pos.x, 10, 0);
            lineH.SetPosition(0, new Vector3(pos.x, 10, 0));
            lineH.SetPosition(1, new Vector3(pos.x, 10, pos.z));
            linewTe.text = pos.x.ToString("f2");
            linewTe.transform.position = pos + (p1 - pos) * 0.5f;
            linehTe.text = pos.z.ToString("f2");
            linehTe.transform.position = pos + (p2 - pos) * 0.5f;
        }
        else if (Physics.Raycast(mainCa.ScreenPointToRay(Input.mousePosition), out hit, maxDistance))
        {
            if (!drag && hit.transform != lasthit)
            {
                if (lastO)
                    lastO.enabled = false;
                lasthit = hit.transform;
                lastO = lasthit.GetComponent<Outline>();
                if (!lastO)
                    lastO = lasthit.gameObject.AddComponent<Outline>();
                lastO.enabled = true;
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (lastO)
                {
                    lastmousePos = new Vector3(hit.point.x, 0, hit.point.z);
                    lastTrpos = lastO.transform.position;
                    lineW.gameObject.SetActive(true);
                    lineH.gameObject.SetActive(true);
                    drag = true;
                }
            }
        }
        else
        {
            if (lastO)
            {
                lastO.enabled = false;
                lastO = null;
            }
        }
    }

}
