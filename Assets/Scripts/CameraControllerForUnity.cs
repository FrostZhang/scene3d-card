using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CameraControllerForUnity : MonoBehaviour
{
    public static CameraControllerForUnity Instance;
    public Transform followtarget;
    [Header("接受unity输入")]
    public bool canUseMouseRight = true;
    public bool canUseMouseCenter = true;
    public bool canUseMouseScroll = true;
    public bool useCustomMouse_xy = false;
    public bool enableUIInput = false;
    [Header("最小视距")]
    public float minFarClipPlane = 5000;
    [Header("第三 设定距离")]
    public float curdistance;
    public float targetdis;
    [Header("正交")]
    private float curorthographicSize;
    public float targetorthographicSize;
    [Header("三 一 moba")]
    public Mode mode = Mode.third;
    [Header("第三人称镜头角度 注意xy是反的")]
    public float xAngle, yAngle;
    float xspeed = 2;   //放大系数
    float yspeed = 2;
    [Header("第三 限制上下角度")]
    public Vector2 thirdLimitAngleY = new Vector2(-45, 90);

    [Header("限制 moba广角高度")]
    public Vector2 mobaLimitDistance = new Vector2(2, 500);

    [Header("限制 正交")]
    public Vector2 orthographicSizeLimit = new Vector2(2, 500);

    [Header("镜头锁定 偏移")]
    public Vector3 offset;

    [Header("第一人称fov")]
    public float targetfov;

    [Header("第一三 视距平滑"), Range(0, 1)]
    public float widenSmooth = 0.2f;
    [Header("旋转平滑"), Range(0, 1)] 
    public float roSmooth = 0.1f;

    /// <summary>
    /// 自动跟随followtarget 转动相机
    /// </summary>
    [Header("第一三跟随 自动转动")]
    public bool autoRotate;
    [Header("第一三 自动转动平滑"), Range(0.5f, 3)]
    public float autoRotateSmooth = 1;

    public Transform focus;
    Quaternion rot;
    Quaternion pivotRot;

    float normalnearClipPlane;
    protected Camera came;
    private Transform ca;
    private Vector3 triniPivot;
    protected virtual void Awake()
    {
        Instance = this;
        triniPivot = transform.rotation.eulerAngles;
        yAngle = triniPivot.x;
        xAngle = triniPivot.y;
        came = GetComponentInChildren<Camera>();
        ca = came.transform;
        ca.LookAt(transform.position);
        targetdis = curdistance = Vector3.Distance(transform.position, ca.position);
        targetfov = came.fieldOfView;
        normalnearClipPlane = came.nearClipPlane;
        curorthographicSize = targetorthographicSize = came.orthographicSize;
    }

    float mousex;
    float mousey;
    float mousez;
    bool mobaMoveFlag;
    float checkAutoRo = 0;
    const float checkInterval = 4;
    float ycurrentVelocity;
    float xcurrentVelocity;
    Vector3 lastMousepos;
    void LateUpdate()
    {
        if (!enableUIInput && EventSystem.current.IsPointerOverGameObject())
        {
            mousex = 0;
            mousey = 0;
            mousez = 0;
            mobaMoveFlag = false;
        }
        else
        {
            if (useCustomMouse_xy)
            {
                var offset = (Input.mousePosition - lastMousepos) * 50;
                lastMousepos = Input.mousePosition;
                mousex = offset.x / Screen.safeArea.width;
                mousey = offset.y / Screen.safeArea.height;
            }
            else
            {
                mousex = Input.GetAxis("Mouse X");
                mousey = Input.GetAxis("Mouse Y");
            }
            if (canUseMouseScroll)
                mousez = Input.GetAxis("Mouse ScrollWheel");
            else
                mousez = 0;
        }

        #region 旋转
        CameraRotate();
        #endregion
        #region 视角缩进，拉远，跟随
        CameraZoom();
        #endregion

        #region 视角平移
        CameraMove();
        #endregion
    }

    private void CameraZoom()
    {
        if (mode == Mode.third || mode == Mode.moba)
        {
            if (mousez != 0)
            {
                if (!came.orthographic)
                {
                    //相机离的越远越快
                    targetdis = Mathf.Clamp(curdistance + mousez * (curdistance * 0.66f + 6.6f), mobaLimitDistance.x, mobaLimitDistance.y);
                    SetFarClipPlane(targetdis * 40.5f + 240f);
                    if (focus)  //改变焦点
                    {
                        var sc = targetdis * .03f;
                        focus.localScale = new Vector3(sc, sc, sc);
                    }
                }
                else
                {
                    targetorthographicSize = Mathf.Clamp(targetorthographicSize +
                        mousez * (targetorthographicSize * 0.22f + 2.2f), orthographicSizeLimit.x, orthographicSizeLimit.y);
                    SetFarClipPlane(targetorthographicSize + 250f);
                    if (focus)  //改变焦点
                    {
                        var sc = targetorthographicSize * .03f;
                        focus.localScale = new Vector3(sc, sc, sc);
                    }
                }
            }
            //结算 相机与物体的距离
            if (came.orthographic)
            {
                //正交模式 改变size 
                came.orthographicSize = curorthographicSize = Mathf.Lerp(came.orthographicSize, targetorthographicSize, roSmooth);
                ca.position = transform.position - ca.forward.normalized * targetdis;
            }
            else
            {
                //广角模式 改变距离
                targetdis = Mathf.Clamp(targetdis, mobaLimitDistance.x, mobaLimitDistance.y);
                curdistance = Mathf.Lerp(curdistance, targetdis, 5 * 0.02f);
                ca.position = transform.position - ca.forward.normalized * curdistance;
            }

            if (followtarget)
            {
                //跟随
                transform.position = Vector3.Lerp(transform.position, followtarget.position + offset, widenSmooth);
            }
        }
        else if (mode == Mode.first)
        {
            //第一人称跟随
            if (followtarget)
            {
                transform.position = followtarget.position;
                //第一视角锁死下  可变fov
                targetfov += mousez * 3;
                targetfov = Mathf.Clamp(targetfov, 30, 70);
            }
            else
            {
                transform.position -= (transform.forward).normalized * mousez * 3;
            }
            ca.position = transform.position + ca.forward.normalized * 0.05f;   //camera is too far with people . fixed the dis;
        }
        //相机广角fov 平滑
        came.fieldOfView = Mathf.Lerp(came.fieldOfView, targetfov, 0.2f);
    }

    private void CameraMove()
    {
        if (mode == 0)
        {
            if (Input.GetMouseButton(0) && Input.GetMouseButton(1) && canUseMouseCenter)
            {
                if (mode == Mode.first)
                {
                }
                    mousex = -mousex;
                    mousey = -mousey;
                Vector3 move = new Vector3(mousex, mousey, 0) * curdistance * 0.03f;
                transform.Translate(move, Space.Self);
                if (followtarget)
                {
                    offset = transform.position - followtarget.position;
                }
            }
        }
        else if (mode == Mode.moba)
        {
            if (Input.GetMouseButton(2) && canUseMouseCenter)
            {
                if (came.orthographic)
                    transform.Translate(new Vector3(curorthographicSize * 0.06f * mousex, 0, curorthographicSize * 0.06f * mousey), Space.World);
                else
                    transform.Translate(new Vector3(curdistance * 0.06f * mousex, 0, curdistance * 0.06f * mousey), Space.World);
                if (followtarget)
                    offset = transform.position - followtarget.position;
            }
        }
    }

    private void CameraRotate()
    {
        //当不为moba视角可旋转
        if (Input.GetMouseButton(0) && !Input.GetMouseButton(1) &&(mode == Mode.third || mode == Mode.first))
        {
            if (canUseMouseRight)
            {
                if (mousex != 0 || mousey != 0)
                {
                    checkAutoRo = checkInterval;
                }
                xAngle += mousex * xspeed;
                yAngle -= mousey * yspeed;
                yAngle = Mathf.Clamp(yAngle, thirdLimitAngleY.x, thirdLimitAngleY.y);
            }
        }
        rot = Quaternion.Euler(0f, xAngle, 0f);
        pivotRot = Quaternion.Euler(yAngle, triniPivot.y, triniPivot.z);
        //m_Pivot.localRotation = Quaternion.Slerp(m_Pivot.localRotation, m_PivotTargetRot, m_TurnSmoothing * 0.02f);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot * pivotRot, roSmooth);
        if (autoRotate && followtarget)
        {
            if ((checkAutoRo -= Time.deltaTime) < 0)
            {
                checkAutoRo = 0;
                if (mode == Mode.third)
                {
                    Quaternion rot = transform.rotation;
                    Quaternion toTarget = Quaternion.LookRotation(followtarget.forward);
                    var to = (Quaternion.Euler(0, -triniPivot.y, 0) * toTarget).eulerAngles.y;
                    xAngle = to;
                }
                else if (mode == Mode.first)
                {
                    transform.rotation = followtarget.rotation;
                    xAngle = transform.eulerAngles.y;
                    yAngle = transform.eulerAngles.x;
                }
            }
        }
    }

    private void SetFarClipPlane(float f)
    {
        came.farClipPlane = Mathf.Max(minFarClipPlane, f);
    }

    public void MobaFollow_orthogonal(Transform target, Vector2 angle, float dis, float size)
    {
        mode = Mode.moba;
        followtarget = target;
        xAngle = angle.y;
        yAngle = angle.x;
        targetfov = 60;
        widenSmooth = 1;
        offset = Vector3.zero;
        came.nearClipPlane = normalnearClipPlane;
        came.orthographic = true;
        targetdis = Mathf.Clamp(dis, mobaLimitDistance.x, mobaLimitDistance.y);
        targetorthographicSize = size < 1 ? 1 : size;
    }

    public void SetRoSpeed(float speed)
    {
        xspeed = speed;
        yspeed = speed;
    }

    public void MobaFollow(Transform target, Vector2 angle, float dis)
    {
        mode = Mode.moba;
        came.orthographic = false;
        followtarget = target;
        xAngle = angle.y;
        yAngle = angle.x;
        targetdis = Mathf.Clamp(dis, mobaLimitDistance.x, mobaLimitDistance.y);
        targetfov = 60;
        widenSmooth = 1;
        offset = Vector3.zero;
        came.nearClipPlane = normalnearClipPlane;
        SetFarClipPlane(targetdis * 40.5f + 240f);
    }

    public void Mobafocus(Vector3 pos, Vector2 angle, float dis)
    {
        mode = Mode.moba;
        came.orthographic = false;
        followtarget = null;
        transform.position = pos;
        xAngle = angle.y;
        yAngle = angle.x;
        targetdis = Mathf.Clamp(dis, mobaLimitDistance.x, mobaLimitDistance.y);
        targetfov = 60;
        widenSmooth = 0.2f;
        offset = Vector3.zero;
        came.nearClipPlane = normalnearClipPlane;
        SetFarClipPlane(targetdis * 40.5f + 240f);
    }

    public void Mobafocus(Vector2 angle, float dis)
    {
        Mobafocus(transform.position, angle, dis);
    }

    public void Thirdfocus(Vector3 pos, Vector2 angle, float dis)
    {
        mode = Mode.third;
        came.orthographic = false;
        followtarget = null;
        transform.position = pos;
        xAngle = angle.y;
        yAngle = angle.x;
        targetdis = Mathf.Clamp(dis, mobaLimitDistance.x, mobaLimitDistance.y);
        targetfov = 60;
        widenSmooth = 0.2f;
        offset = Vector3.zero;
        came.nearClipPlane = normalnearClipPlane;
        SetFarClipPlane(targetdis + 250f);
    }

    public void Thirdfocus(Vector2 angle, float dis)
    {
        Thirdfocus(transform.position, angle, dis);
    }

    public void ThirdFollow(Transform target, Vector2 angle, float dis)
    {
        mode = Mode.third;
        came.orthographic = false;
        followtarget = target;
        xAngle = angle.y;
        yAngle = angle.x;
        targetdis = Mathf.Clamp(dis, mobaLimitDistance.x, mobaLimitDistance.y);
        targetfov = 60;
        //紧跟物体，不平滑，防止物体走的时候相机抖动。
        widenSmooth = 1f;
        offset = Vector3.zero;
        came.nearClipPlane = normalnearClipPlane;
        SetFarClipPlane(targetdis + 250f);
    }

    public void Firstfocus(Vector3 pos, Vector2 angle)
    {
        mode = Mode.first;
        came.orthographic = false;
        followtarget = null;
        transform.position = pos;
        xAngle = angle.y;
        yAngle = angle.x;
        targetfov = 60;
        widenSmooth = 1f;
        offset = Vector3.zero;
        came.nearClipPlane = 0.3f;
        SetFarClipPlane(minFarClipPlane);
    }

    public void FirstFollow(Transform target, Vector2 angle)
    {
        mode = Mode.first;
        came.orthographic = false;
        followtarget = target;
        targetfov = 60;
        xAngle = angle.y;
        yAngle = angle.x;
        //紧跟物体，不平滑，防止物体走的时候相机抖动。
        widenSmooth = 1f;
        offset = Vector3.zero;
        came.nearClipPlane = 0.3f;
        SetFarClipPlane(minFarClipPlane);
    }

    public enum Mode
    {
        third,
        first,
        moba
    }
}
#if UNITY_EDITOR

[CustomEditor(typeof(CameraControllerForUnity))]
public class CameraControllerE : Editor
{
    Transform targetforca;
    Vector2 angle;
    float dis;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("------------   以下为测试   ------------");
        targetforca = EditorGUILayout.ObjectField("target", targetforca, typeof(Transform), true) as Transform;
        angle = EditorGUILayout.Vector2Field("angle", angle);
        dis = EditorGUILayout.FloatField("dis", dis);
        if (GUILayout.Button("第三人称切换"))
        {
            (target as CameraControllerForUnity).ThirdFollow(targetforca, angle, dis);
        }
        if (GUILayout.Button("第一人称切换"))
        {
            (target as CameraControllerForUnity).FirstFollow(targetforca, angle);
        }
        if (GUILayout.Button("moba切换"))
        {
            (target as CameraControllerForUnity).MobaFollow(targetforca, angle, dis);
        }
        if (GUILayout.Button("moba正交切换"))
        {
            (target as CameraControllerForUnity).MobaFollow_orthogonal(targetforca, angle, dis, dis);
        }
        Undo.RecordObject(target, target.name);
    }
}

#endif
