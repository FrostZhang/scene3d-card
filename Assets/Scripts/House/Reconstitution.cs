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
    Transform caTr;
    Transform rigTr;
    Transform cs;
    private RaycastHit hit;
    private float maxDistance = 25;
    private Transform lasthit;
    Outline lastO;
    Vector3 lastmousePos, lastTrpos;
    bool drag;
    public Color lineColor = new Color(1, 1, 1, 0.5f);
    public Material lineMat;
    public LineRenderer lineW;
    public TextMesh linewTe;
    public LineRenderer lineH;
    public TextMesh linehTe;

    Dictionary<Transform, Bounds> bounds;
    void Start()
    {
        Instance = this;
        mainCa = Camera.main;
        caTr = mainCa.transform;
        rigTr = CameraControllerForUnity.Instance.transform;

        RTEditor.EditorObjectSelection.Instance.SelectionChanged += Instance_SelectionChanged;
    }

    private void Instance_SelectionChanged(RTEditor.ObjectSelectionChangedEventArgs selectionChangedEventArgs)
    {
        if (selectionChangedEventArgs.SelectedObjects.Count > 0)
        {
            CameraControllerForUnity.Instance.canUseMouseCenter = false;
        }
        else
        {
            CameraControllerForUnity.Instance.canUseMouseCenter = true;
        }
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
            PropPanle.Instance.Show(false);
            BoundsPanel.Instance.Bingding(null, null);
            return;
        }
        isEdit = true;
        rigTr.position = new Vector3(0, -1, 0);
        CameraControllerForUnity.Instance.MobaFollow_orthogonal(rigTr, new Vector2(90, 0), 15, 5);
        CameraControllerForUnity.Instance.orthographicSizeLimit = new Vector2(2, 10);
        cs = GetComponent<House>().parent;
        bounds = new Dictionary<Transform, Bounds>(cs.childCount);
        foreach (Transform item in cs)
        {
            var e = item.GetComponent<HassEntity>();
            if (e)
            {
                e.ReconstitutionMode(true);
                if (e.editC)
                {
                    var bds = e.editC.bounds;
                    bounds.Add(item, bds);
                }
            }
            else
            {
                var c = item.GetComponent<Collider>();
                if (c)
                {
                    c.enabled = true;
                    var bds = c.bounds;
                    bounds.Add(item, bds);
                }
            }
        }
    }

    RaycastHit[] jiances;
    float interval;
    void Update()
    {
        if (!isEdit || EventSystem.current.IsPointerOverGameObject())
            return;
        if (drag)
        {
            if (Input.GetMouseButtonUp(0))
            {
                drag = false;
                lineW.gameObject.SetActive(false);
                lineH.gameObject.SetActive(false);
                //CameraControllerForUnity.Instance.canUseMouseCenter = true;
                return;
            }
            //if ((interval -= Time.deltaTime) < 0)
            {
                //interval = 0.03f;
                var pos = mainCa.ScreenToWorldPoint(Input.mousePosition);
                pos.y = 0;
                pos = lasthit.position = lastTrpos + (pos - lastmousePos);
                PropPanle.Instance.Flush(new int[] { 0, 1 }, pos.x, pos.z);

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

                //BoundsPanel.Instance.FlushAll(hit.collider.bounds);
            }

        }
        else if (Physics.Raycast(mainCa.ScreenPointToRay(Input.mousePosition), out hit, maxDistance))
        {
            if (!drag && hit.transform != lasthit)
            {
                if (lastO)
                    lastO.enabled = false;
                lasthit = hit.transform;
                //lastO = lasthit.GetComponent<Outline>();
                //if (!lastO)
                //    lastO = lasthit.gameObject.AddComponent<Outline>();
                //lastO.enabled = true;
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (lasthit)
                {
                    ShowDetail(lasthit);
                    //BoundsPanel.Instance.Bingding(lasthit, hit.collider);
                    //RTEditor.EditorObjectSelection.Instance.FixedSelectObj(lasthit, RTEditor.GizmoType.VolumeScale);
                    //lastTrpos = lasthit.position;
                    //lastmousePos = new Vector3(hit.point.x, 0, hit.point.z);
                    //lineW.gameObject.SetActive(true);
                    //lineH.gameObject.SetActive(true);
                    //drag = true;
                    //CameraControllerForUnity.Instance.canUseMouseCenter = false;
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
            else if (Input.GetMouseButtonDown(0))
            {
                PropPanle.Instance.Show(false);
                BoundsPanel.Instance.Bingding(null, null);
            }
        }
    }

    void ShowDetail(Transform target)
    {
        if (House.Instance.CureetHouse.ContainsKey(target))
        {
            var t = House.Instance.CureetHouse[target];
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

    private void ShowAppliances(Transform target)
    {
        PropPanle.Instance.Clear();
        PropPanle.Instance.GetV3("Pos", target.position.x, target.position.z, target.position.y,
            (x) =>
            {
                var pc = target.position;
                pc.x = x;
                target.position = pc;
            }, (x) =>
            {
                var pc = target.position;
                pc.z = x;
                target.position = pc;
            }, (x) =>
            {
                var pc = target.position;
                pc.y = x;
                target.position = pc;
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
        }, (x) =>
        {
            var pc = target.position;
            pc.z = x;
            target.position = pc;
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
        PropPanle.Instance.GetEntity("ID", door.Entity_id, (x) =>
        {
            door.SetEntity(x);
        });
        PropPanle.Instance.Show(true);
    }

    private void ShowWallFloor(Transform target)
    {
        PropPanle.Instance.Clear();
        PropPanle.Instance.GetV2("Pos", target.position.x, target.position.z, (x) =>
        {
            var pc = target.position;
            pc.x = x;
            target.position = pc;
        }, (x) =>
        {
            var pc = target.position;
            pc.z = x;
            target.position = pc;
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
        PropPanle.Instance.GetV2("Pos", target.position.x, target.position.z, (x) =>
        {
            var pc = target.position;
            pc.x = x;
            target.position = pc;
        }, (x) =>
        {
            var pc = target.position;
            pc.z = x;
            target.position = pc;
        });
        PropPanle.Instance.GetV1("W", target.localScale.x, (x) =>
         {
             var pc = target.localScale;
             if (x == 0) x = 0.1f;
             pc.x = x;
             target.localScale = pc;
         });
        PropPanle.Instance.GetV2("O/C", target.position.x, target.position.z, (x) =>
        {
            door.angleopen = x;
        }, (x) =>
        {
            door.angleclose = x;
        });
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
        }, (x) =>
        {
            var pc = target.position;
            pc.z = x;
            target.position = pc;
        }, (x) =>
        {
            var pc = target.position;
            pc.y = x;
            target.position = pc;
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

    private static void ShowWallProp(Transform target)
    {
        PropPanle.Instance.Clear();
        PropPanle.Instance.GetV2("Pos", target.position.x, target.position.z, (x) =>
        {
            var pc = target.position;
            pc.x = x;
            target.position = pc;
        }, (x) =>
        {
            var pc = target.position;
            pc.z = x;
            target.position = pc;
        });
        PropPanle.Instance.GetV2("W/H", target.localScale.x, target.localScale.y, (x) =>
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
            pc = target.position;
            pc.y = x * 0.5f;
            target.position = pc;
        });
        PropPanle.Instance.GetV1("T", target.localScale.z, (x) =>
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
        PropPanle.Instance.Show(true);
    }

}
