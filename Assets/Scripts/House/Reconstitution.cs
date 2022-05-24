using System;
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
    //Transform caTr;
    Transform rigTr;
    Transform cs;
    private RaycastHit hit;
    private float maxDistance = 25;
    private Transform lasthit;
    //Outline lastO;
    Vector3 lastmousePos, lastTrpos;
    bool drag;
    public Color lineColor = new Color(1, 1, 1, 0.5f);
    public Material lineMat;
    public LineRenderer lineW;
    public TextMesh linewTe;
    public LineRenderer lineH;
    public TextMesh linehTe;

    CHHandle chhadle;
    List<RTEditor.Gizmo> ligizmo;
    Vector3 offset = new Vector3(1, 0, 1) * 0.125f;
    void Start()
    {
        Instance = this;
        mainCa = Camera.main;

        chhadle = new CHHandle();
        //caTr = mainCa.transform;
        rigTr = CameraControllerForUnity.Instance.transform;
        ligizmo = new List<RTEditor.Gizmo>(5);
        PropPanle.Instance.Ondel = OnDelete;
    }

    private void OnDelete()
    {
        CameraControllerForUnity.Instance.canUseMouseCenter = true;
        ClearHandle();
        House.Instance.Delete(lasthit);
        PropPanle.Instance.Show(false);
        lasthit = null;
    }

    private void VolumeScaleGizmo_GizmoDragUpdate(RTEditor.Gizmo gizmo)
    {
        FlushPropPanle();
    }

    private void TranslationGizmo_GizmoDragUpdate(RTEditor.Gizmo gizmo)
    {
        FlushPropPanle();
    }


    public void OpenRTEditor()
    {

    }

    public void CloseRTEditor()
    {
        PropPanle.Instance.Show(false);
        lasthit = null;
        ClearHandle();
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
            //BoundsPanel.Instance.Bingding(null, null);
            CloseRTEditor();
            CreatPanel.Instance.Show(false);
            return;
        }
        isEdit = true;
        CreatPanel.Instance.Show(true);
        rigTr.position = new Vector3(0, -1, 0);
        CameraControllerForUnity.Instance.MobaFollow_orthogonal(rigTr, new Vector2(90, 0), 15, 5);
        CameraControllerForUnity.Instance.orthographicSizeLimit = new Vector2(2, 10);
        cs = GetComponent<House>().parent;
        //bounds = new Dictionary<Transform, Bounds>(cs.childCount);
        foreach (Transform item in cs)
        {
            var e = item.GetComponent<HassEntity>();
            if (e) e.ReconstitutionMode(true);
            else
            {
                var c = item.GetComponent<Collider>();
                if (c) c.enabled = true;
            }
        }

        OpenRTEditor();

    }

    public void EditLine(LineEntity e, Action<Color> onchangeC, Action Ondel)
    {
        CameraControllerForUnity.Instance.canUseMouseCenter = false;
        ClearHandle();
        PropPanle.Instance.Clear();
        var l = e.line;
        for (int i = 0; i < l.positionCount; i++)
        {
            var n = i;
            var p = l.GetPosition(n);
            var m = chhadle.PositionHandle(p, Quaternion.identity, (x) =>
             {
                 var pn = l.GetPosition(n);
                 pn += x;
                 l.SetPosition(n, pn);
                 PropPanle.Instance.Flush(new int[] { n * 3, n * 3 + 1, n * 3 + 2 }, pn.x, pn.z, pn.y);
             });
            m.MultiAxisSquareSize = 0;
            m.AxisLength = 1;
            m.SetAxisVisibility(false, 1);
            ligizmo.Add(m);
            PropPanle.Instance.GetV3(n.ToString(), p.x, p.z, p.y, (x) =>
            {
                var pn = l.GetPosition(n);
                pn.x = x;
                l.SetPosition(n, pn);
                m.transform.position = pn;
            }, (y) =>
            {
                var pn = l.GetPosition(n);
                pn.z = y;
                l.SetPosition(n, pn);
                m.transform.position = pn;
            }, (z) =>
            {
                var pn = l.GetPosition(n);
                pn.y = z;
                l.SetPosition(n, pn);
                m.transform.position = pn;
            });
        }
        PropPanle.Instance.GetColor("on", e.Oncolor, (x) =>
        {
            e.SetOnc(x);
            onchangeC?.Invoke(x);
        });
        PropPanle.Instance.GetColor("off", e.Offcolor, (x) =>
        {
            e.SetOffc(x);
        });
        lasthit = l.transform;
        PropPanle.Instance.GetEntity("Entity", e.Entity_id, (x) =>
        {
            e.SetEntity(x);
        });
        Action action = null;
        action = () =>
       {
           PropPanle.Instance.Ondel -= action;
           Ondel?.Invoke();
       };
        PropPanle.Instance.Ondel += action;
        PropPanle.Instance.Show(true);
    }

    RaycastHit[] jiances;
    float interval;
    void Update()
    {
        if (!isEdit || EventSystem.current.IsPointerOverGameObject())
            return;
        if (Input.GetMouseButtonDown(0))
        {
            foreach (var item in ligizmo)
            {
                if (item.IsReadyForObjectManipulation())
                {
                    CameraControllerForUnity.Instance.canUseMouseCenter = false;
                    return;
                }
            }
            if (Physics.Raycast(mainCa.ScreenPointToRay(Input.mousePosition), out hit, maxDistance))
            {
                if (hit.transform != lasthit)
                {
                    lasthit = hit.transform;
                    ClearHandle();
                    SelectTr(lasthit);
                }
            }
            else
            {
                ClearHandle();
                lasthit = null;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            CameraControllerForUnity.Instance.canUseMouseCenter = true;
        }
    }

    private void ClearHandle()
    {
        foreach (var item in ligizmo)
            chhadle.ReHandle(item);
        ligizmo.Clear();
    }

    public void EditorTransform(Transform obj)
    {
        if (lasthit != obj)
        {
            lasthit = obj;
            ClearHandle();
            SelectTr(lasthit);
        }
    }

    private void SelectTr(Transform lasthit)
    {
        var h = chhadle.PositionHandle(lasthit.position + offset, Quaternion.identity, (x) =>
             {
                 lasthit.position += x;
                 FlushPropPanle();
             });
        h.SetAxisVisibility(false, 1);
        ligizmo.Add(h);
        var s = chhadle.VolumeHandle(lasthit.gameObject, (a, b) =>
         {
             lasthit.localScale = a;
             lasthit.position = b;
             h.transform.position = lasthit.position + offset;
             FlushPropPanle();
         });
        s.SetAxisVisibility(false, 1);
        ligizmo.Add(s);
        ShowDetail(lasthit);
    }

    HouseEntityType cureetET;
    void ShowDetail(Transform target)
    {
        if (House.Instance.CureetHouse.ContainsKey(target))
        {
            var t = House.Instance.CureetHouse[target];
            cureetET = t;
            switch (t)
            {
                case HouseEntityType.wall:
                    ShowWallProp(target);
                    break;
                case HouseEntityType.floor:
                    ShowWallFloor(target);
                    break;
                case HouseEntityType.door:
                    ShowDoor(target);
                    break;
                case HouseEntityType.stand:
                    ShowStand(target);
                    break;
                //case HouseEntityType.sky:
                //    break;
                case HouseEntityType.arealight:
                    ShowAreaLight(target);
                    break;
                case HouseEntityType.appliances:
                    ShowAppliances(target);
                    break;
                //case HouseEntityType.flowLine:
                //    break;
                //case HouseEntityType.animal:
                //    break;
                //case HouseEntityType.weather:
                //    break;
                default:
                    PropPanle.Instance.Show(false);
                    break;
            }
        }
    }

    private void FlushPropPanle()
    {
        switch (cureetET)
        {
            case HouseEntityType.wall:
                var p = lasthit.position;
                var sc = lasthit.localScale;
                PropPanle.Instance.Flush(new int[] { 0, 1, 2, 3, 4, 5 }, p.x, p.z, p.y, sc.x, sc.y, sc.z);
                break;
            case HouseEntityType.floor:
                p = lasthit.position;
                sc = lasthit.localScale;
                PropPanle.Instance.Flush(new int[] { 0, 1, 2, 3 }, p.x, p.z, sc.x, sc.z);
                break;
            case HouseEntityType.door:
                p = lasthit.position;
                sc = lasthit.localScale;
                PropPanle.Instance.Flush(new int[] { 0, 1, 2, 3, 4, 5 }, p.x, p.z, p.y, sc.x, sc.y, sc.z);
                break;
            case HouseEntityType.stand:
                p = lasthit.position;
                sc = lasthit.localScale;
                PropPanle.Instance.Flush(new int[] { 0, 1, 2, 6, 7, 8 }, p.x, p.z, p.y, sc.x, sc.y, sc.z);
                break;
            case HouseEntityType.sky:
                break;
            case HouseEntityType.arealight:
                p = lasthit.position;
                sc = lasthit.localScale;
                PropPanle.Instance.Flush(new int[] { 0, 1, 2, 3 }, p.x, p.z, sc.x, sc.z);
                break;
            case HouseEntityType.appliances:
                p = lasthit.position;
                sc = lasthit.localScale;
                PropPanle.Instance.Flush(new int[] { 0, 1, 2, 6, 7, 8 }, p.x, p.z, p.y, sc.x, sc.y, sc.z);
                break;
            case HouseEntityType.flowLine:
                break;
            case HouseEntityType.animal:
                break;
            case HouseEntityType.weather:
                break;
            default:
                break;
        }
    }

    private void ShowAppliances(Transform target)
    {
        PropPanle.Instance.Clear();
        PropPanle.Instance.GetV3("Pos", target.position.x, target.position.z, target.position.y,
            (x) =>
            {
                var pc = target.position;
                pc.x = x;
                target.position = pc;
                FlushGizmo(pc);
            }, (x) =>
            {
                var pc = target.position;
                pc.z = x;
                target.position = pc;
                FlushGizmo(pc);
            }, (x) =>
            {
                var pc = target.position;
                pc.y = x;
                target.position = pc;
                FlushGizmo(pc);
            });
        PropPanle.Instance.GetV3("Rot", target.localEulerAngles.x, target.localEulerAngles.y, target.localEulerAngles.z,
            (x) =>
            {
                var pc = target.localEulerAngles;
                pc.x = x;
                target.localEulerAngles = pc;
            }, (x) =>
            {
                var pc = target.localEulerAngles;
                pc.y = x;
                target.localEulerAngles = pc;
            }, (x) =>
            {
                var pc = target.localEulerAngles;
                pc.z = x;
                target.localEulerAngles = pc;
            });
        PropPanle.Instance.GetV3("Scale", target.localScale.x, target.localScale.y, target.localScale.z,
            (x) =>
            {
                var pc = target.localScale;
                if (x == 0) x = 0.1f;
                pc.x = x;
                target.localScale = pc;
            }, (x) =>
            {
                var pc = target.localScale;
                if (x == 0) x = 0.1f;
                pc.y = x;
                target.localScale = pc;
            }, (x) =>
            {
                var pc = target.localScale;
                if (x == 0) x = 0.1f;
                pc.z = x;
                target.localScale = pc;
            });
        var door = target.GetComponent<HassEntity>();
        if (door is AlarmEntity)
        {
            var e = door as AlarmEntity;
            var ma = e.YuanMa;
            PropPanle.Instance.GetColor("Main", ma.GetColor("_Color"), (x) => ma.SetColor("_Color", x));
            //PropPanle.Instance.GetColor("Edge", ma.GetColor("_HeightLight"), (x) => ma.SetColor("_HeightLight", x));
            //PropPanle.Instance.GetColor("Depth", ma.GetColor("_DepthColor"), (x) => ma.SetColor("_DepthColor", x));
        }
        else if (door is LampEntity)
        {
            var e = door as LampEntity;
            PropPanle.Instance.GetColor("Main", e.emColor, (x) => e.emColor = x);
        }
        else if (door is TextEntity)
        {
            var e = door as TextEntity;
            PropPanle.Instance.GetText("Front", e.front, (x) =>
            {
                e.front = x;
                e.FlushText();
            });
            PropPanle.Instance.GetText("Back", e.back, (x) =>
            {
                e.back = x;
                e.FlushText();
            });
            PropPanle.Instance.GetColor("Color", e.textMesh.color, (x) => e.textMesh.color = x);
            PropPanle.Instance.GetV1("Size", e.textMesh.fontSize, (x) =>
            {
                e.textMesh.fontSize = (int)x;
            });
        }
        PropPanle.Instance.GetEntity("ID", door.Entity_id, (x) =>
        {
            door.SetEntity(x);
        });
        PropPanle.Instance.Show(true);
    }

    private void ShowAreaLight(Transform target)
    {
        var door = target.GetComponent<LightEntity>();
        PropPanle.Instance.Clear();
        PropPanle.Instance.GetV2("Pos", target.position.x, target.position.z, (x) =>
        {
            var pc = target.position;
            pc.x = x;
            target.position = pc;
            FlushGizmo(pc);
        }, (x) =>
        {
            var pc = target.position;
            pc.z = x;
            target.position = pc;
            FlushGizmo(pc);
        });
        PropPanle.Instance.GetV2("W/H", target.localScale.x, target.localScale.z, (x) =>
        {
            var pc = target.localScale;
            pc.x = x;
            target.localScale = pc;
        }, (x) =>
        {
            var pc = target.localScale;
            pc.z = x;
            target.localScale = pc;
        });
        PropPanle.Instance.GetV1("MaxL", door.Max, (x) =>
        {
            door.Max = x;
        });
        PropPanle.Instance.GetColor("Light", door.clight.color, (x) => door.clight.color = x);
        PropPanle.Instance.GetEntity("ID", door.Entity_id, (x) =>
        {
            door.SetEntity(x);
        });
        PropPanle.Instance.Show(true);
    }

    private void FlushGizmo(Vector3 pc)
    {
        if (ligizmo.Count > 0)
        {
            ligizmo[0].transform.position = pc + offset;
        }
    }

    private void ShowWallFloor(Transform target)
    {
        PropPanle.Instance.Clear();
        PropPanle.Instance.GetV2("Pos", target.position.x, target.position.z, (x) =>
        {
            var pc = target.position;
            pc.x = x;
            target.position = pc;
            FlushGizmo(pc);
        }, (x) =>
        {
            var pc = target.position;
            pc.z = x;
            target.position = pc;
            FlushGizmo(pc);
        });
        PropPanle.Instance.GetV2("W/H", target.localScale.x, target.localScale.z, (x) =>
        {
            var pc = target.localScale;
            pc.x = x;
            target.localScale = pc;
        }, (x) =>
        {
            var pc = target.localScale;
            pc.z = x;
            target.localScale = pc;
        });
        var ma = target.GetComponent<MeshRenderer>().material;
        PropPanle.Instance.GetV2("Txy", ma.GetTextureScale("_BaseMap").x, ma.GetTextureScale("_BaseMap").y, (x) =>
        {
            var s = ma.GetTextureScale("_BaseMap");
            ma.SetTextureScale("_BaseMap", new Vector2(x, s.y));
        }, (x) =>
        {
            var s = ma.GetTextureScale("_BaseMap");
            ma.SetTextureScale("_BaseMap", new Vector2(s.x, x));
        });
        PropPanle.Instance.Show(true);
    }

    private void ShowDoor(Transform target)
    {
        var door = target.GetComponent<DoorEntity>();
        PropPanle.Instance.Clear();
        PropPanle.Instance.GetV3("Pos", target.position.x, target.position.z, target.position.y, (x) =>
        {
            var pc = target.position;
            pc.x = x;
            target.position = pc;
            FlushGizmo(pc);
        }, (x) =>
        {
            var pc = target.position;
            pc.z = x;
            target.position = pc;
            FlushGizmo(pc);
        }, (x) =>
        {
            var pc = target.position;
            pc.y = x;
            target.position = pc;
            FlushGizmo(pc);
        });
        PropPanle.Instance.GetV3("Scale", target.localScale.x, target.localScale.y, target.localScale.z,
            (x) =>
            {
                var pc = target.localScale;
                if (x == 0) x = 0.1f;
                pc.x = x;
                target.localScale = pc;
            }, (x) =>
            {
                var pc = target.localScale;
                if (x == 0) x = 0.1f;
                pc.y = x;
                target.localScale = pc;
            }, (x) =>
            {
                var pc = target.localScale;
                if (x == 0) x = 0.1f;
                pc.z = x;
                target.localScale = pc;
            });
        PropPanle.Instance.GetV1("Close", target.eulerAngles.y, (x) =>
         {
             var pc = target.eulerAngles;
             pc.y = x;
             target.eulerAngles = pc;
         });
        if (door is DoorEntitySliding)
        {

        }
        else
        {
            PropPanle.Instance.GetV1("Open", door.angleopen, (x) =>
            {
                door.angleopen = x;
            });
        }

        PropPanle.Instance.GetEntity("ID", door.Entity_id, (x) =>
        {
            door.SetEntity(x);
        });
        PropPanle.Instance.Show(true);
    }

    private void ShowStand(Transform target)
    {
        PropPanle.Instance.Clear();
        PropPanle.Instance.GetV3("Pos", target.position.x, target.position.z, target.position.y,
            (x) =>
        {
            var pc = target.position;
            pc.x = x;
            target.position = pc;
            FlushGizmo(pc);
        }, (x) =>
        {
            var pc = target.position;
            pc.z = x;
            target.position = pc;
            FlushGizmo(pc);
        }, (x) =>
        {
            var pc = target.position;
            pc.y = x;
            target.position = pc;
            FlushGizmo(pc);
        });
        PropPanle.Instance.GetV3("Rot", target.localEulerAngles.x, target.localEulerAngles.y, target.localEulerAngles.z,
            (x) =>
        {
            var pc = target.localEulerAngles;
            pc.x = x;
            target.localEulerAngles = pc;
        }, (x) =>
        {
            var pc = target.localEulerAngles;
            pc.y = x;
            target.localEulerAngles = pc;
        }, (x) =>
        {
            var pc = target.localEulerAngles;
            pc.z = x;
            target.localEulerAngles = pc;
        });
        PropPanle.Instance.GetV3("Scale", target.localScale.x, target.localScale.y, target.localScale.z,
            (x) =>
        {
            var pc = target.localScale;
            if (x == 0) x = 0.1f;
            pc.x = x;
            target.localScale = pc;
        }, (x) =>
        {
            var pc = target.localScale;
            if (x == 0) x = 0.1f;
            pc.y = x;
            target.localScale = pc;
        }, (x) =>
        {
            var pc = target.localScale;
            if (x == 0) x = 0.1f;
            pc.z = x;
            target.localScale = pc;
        });


        PropPanle.Instance.Show(true);
    }

    private void ShowWallProp(Transform target)
    {
        PropPanle.Instance.Clear();
        PropPanle.Instance.GetV3("Pos", target.position.x, target.position.z, target.position.y,
             (x) =>
             {
                 var pc = target.position;
                 pc.x = x;
                 target.position = pc;
                 FlushGizmo(pc);
             }, (x) =>
             {
                 var pc = target.position;
                 pc.z = x;
                 target.position = pc;
                 FlushGizmo(pc);
             }, (x) =>
             {
                 var pc = target.position;
                 pc.y = x;
                 target.position = pc;
                 FlushGizmo(pc);
             });
        PropPanle.Instance.GetV3("W/H/T", target.localScale.x, target.localScale.y, target.localScale.z,
            (x) =>
            {
                var pc = target.localScale;
                if (x == 0) x = 0.1f;
                pc.x = x;
                target.localScale = pc;
            }, (x) =>
            {
                var pc = target.localScale;
                if (x == 0) x = 0.1f;
                pc.y = x;
                target.localScale = pc;
            }, (x) =>
            {
                var pc = target.localScale;
                if (x == 0) x = 0.1f;
                pc.z = x;
                target.localScale = pc;
            });
        PropPanle.Instance.GetV1("Angle", target.localEulerAngles.y, (x) =>
        {
            var pc = target.localEulerAngles;
            pc.y = x;
            target.localEulerAngles = pc;
        });
        var r = target.GetComponent<MeshRenderer>().material;
        PropPanle.Instance.GetColor("Color", r.color, (x) => r.color = x);
        PropPanle.Instance.Show(true);
    }

}
