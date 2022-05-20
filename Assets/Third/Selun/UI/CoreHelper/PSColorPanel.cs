using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class PSColorPanel : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    enum EditType
    {
        None,
        Hue,
        Sature,
    }

    [SerializeField]
    GameObject hueThumb;
    [SerializeField]
    GameObject svThumb;
    [SerializeField]
    RectTransform hueRect;
    [SerializeField]
    RectTransform svRect;
    [SerializeField]
    Material matSV;

    EditType curType = EditType.None;
    float currentHue;
    float currentSat;
    float currentBright;
    byte alpha;
    const float RingLen = 0.27f;
    public Action<Color> OnColorChanged;
    public InputField rf, gf, bf, af;
    void Start()
    {
        SetColor(currentHue, currentSat, currentBright);
        rf.onEndEdit.AddListener((x) =>
        {
            if (string.IsNullOrWhiteSpace(x))
                rf.text = "0";
            AnsInputColor32();
        }); gf.onEndEdit.AddListener((x) =>
        {
            if (string.IsNullOrWhiteSpace(x))
                gf.text = "0";
            AnsInputColor32();
        }); bf.onEndEdit.AddListener((x) =>
        {
            if (string.IsNullOrWhiteSpace(x))
                bf.text = "0";
            AnsInputColor32();
        }); af.onEndEdit.AddListener((x) =>
        {
            if (string.IsNullOrWhiteSpace(x))
                af.text = "0";
            AnsInputColor32();
        });
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

    }

    public void OnEndDrag(PointerEventData eventData)
    {

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (curType == EditType.Hue)
        {
            SetHueThumbPos(GetHueFromClickPos());
        }
        else if (curType == EditType.Sature)
        {
            float s, v;
            GetSVFromClickPos(out s, out v);
            SetSatureThumbPos(s, v);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsInHueRing())
        {
            // 点中了环形区域
            curType = EditType.Hue;
            SetHueThumbPos(GetHueFromClickPos());
        }
        else
        {
            // 如果不是色相，再判定饱和度
            if (IsInSVTriangle())
            {
                curType = EditType.Sature;
                float s, v;
                GetSVFromClickPos(out s, out v);
                SetSatureThumbPos(s, v);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        curType = EditType.None;
    }

    #region SVSet
    bool IsInSVTriangle()
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(svRect, Input.mousePosition, null, out local);
        // a,b,c triangle
        Vector2 t = new Vector2(local.x, local.y) * 2 / svRect.rect.size;

        Vector3 p = new Vector3(t.x, t.y, 0);
        Vector3 a = new Vector3(-1, 1, 0);
        Vector3 b = new Vector3(1, 1, 0);
        Vector3 c = new Vector3(0, -0.7320508075688773f, 0); // 1-sqrt(3)

        Vector3 ab = (b - a).normalized;
        Vector3 bc = (c - b).normalized;
        Vector3 ca = (a - c).normalized;
        Vector3 ap = (p - a).normalized;
        Vector3 bp = (p - b).normalized;
        Vector3 cp = (p - c).normalized;

        Vector3 d1 = Vector3.Cross(ab, ap);
        Vector3 d2 = Vector3.Cross(bc, bp);
        Vector3 d3 = Vector3.Cross(ca, cp);

        return Vector3.Dot(d1, d2) > 0 && Vector3.Dot(d2, d3) > 0;
    }

    void GetSVFromClickPos(out float s, out float v)
    {
        s = 0;
        v = 0;

        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(svRect, Input.mousePosition, null, out local);
        Vector2 t = new Vector2(local.x, local.y) / svRect.rect.size + new Vector2(0.5f, 0.5f);

        float sqrt3dv2 = 0.8660254037844386f;
        float oneminus = 1 - sqrt3dv2;

        v = (Mathf.Clamp(t.y, oneminus, 1) - oneminus) / sqrt3dv2;
        float temp = v / 2;
        s = Mathf.Clamp(t.x, 0.5f - temp, 0.5f + temp);
        s = v == 0 ? 0 : (s - 0.5f + temp) / v;

        currentSat = s;
        currentBright = v;

        // Debug.Log($"{currentSat}, {currentBright}");
    }

    void SetSatureThumbPos(float s, float v)
    {
        float sqrt3dv2 = 0.8660254037844386f;
        float oneminus = 1 - sqrt3dv2;
        float y = v * sqrt3dv2 + oneminus; // [0-1]的坐标
        float width = v;
        float x = s * width + 0.5f - width / 2; // [0-1]的坐标

        Vector2 pos = new Vector2(x, y) * svRect.rect.size - svRect.rect.size / 2;

        // Debug.Log($"{x}, {y}, {pos.x}, {pos.y}");
        svThumb.transform.localPosition = new Vector3(pos.x, pos.y, 0);
        UpdatePickColor();
    }
    #endregion

    #region HueSet
    bool IsInHueRing()
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(hueRect, Input.mousePosition, null, out local);
        // 判定是在色相的环里，还是饱和度的三角形里
        Vector2 dir = new Vector2(local.x, local.y) - hueRect.rect.center;
        Vector2 d = new Vector2(dir.x * 2 / hueRect.rect.width, dir.y * 2 / hueRect.rect.height);
        float r = d.magnitude;
        return r >= (1 - RingLen) && r <= 1;
    }

    float GetHueFromClickPos()
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(hueRect, Input.mousePosition, null, out local);
        Vector2 dir = new Vector2(local.x, local.y) - hueRect.rect.center;
        float theta = Mathf.Acos(Vector2.Dot(dir.normalized, Vector2.right));
        if (dir.y < 0)
        {
            theta = 2 * Mathf.PI - theta;
        }
        currentHue = theta / 2 / Mathf.PI;

        return currentHue;
    }

    void UpdatePickColor()
    {
        Color32 color = Color.HSVToRGB(currentHue, currentSat, currentBright);
        rf.text = color.r.ToString();
        gf.text = color.g.ToString();
        bf.text = color.b.ToString();
        color.a = alpha;
        af.text = alpha.ToString();
        if (OnColorChanged != null)
            OnColorChanged(color);
    }

    void SetHueThumbPos(float h)
    {
        float theta = currentHue * Mathf.PI * 2;
        float x = Mathf.Cos(theta);
        float y = Mathf.Sin(theta);
        Vector2 dir = new Vector2(x, y);
        dir = (1 - RingLen * 0.5f) * dir.normalized;
        var offset = new Vector2((hueRect.rect.width) * (0.5f - hueRect.pivot.x), (hueRect.rect.height) * (0.5f - hueRect.pivot.y));
        Vector2 pos = new Vector2(dir.x * hueRect.rect.width * 0.5f + offset.x, dir.y * hueRect.rect.height * 0.5f + offset.y);

        hueThumb.transform.localPosition = new Vector3(pos.x, pos.y, 0);

        matSV.SetFloat("_Hue", currentHue * 360);
        UpdatePickColor();
    }
    #endregion

    public void AnsInputColor32()
    {
        byte r, g, b, a;
        if (!byte.TryParse(rf.text, out r)) return;
        if (!byte.TryParse(gf.text, out g)) return;
        if (!byte.TryParse(bf.text, out b)) return;
        if (!byte.TryParse(af.text, out a)) return;
        SetColor(new Color32(r, g, b, a));
    }

    public void SetColor(Color c)
    {
        float h, s, v;
        alpha = ((Color32)c).a;
        Color.RGBToHSV(c, out h, out s, out v);
        SetColor(h, s, v);
    }
    public void SetColor(float h, float s, float v)
    {
        currentHue = h;
        currentSat = s;
        currentBright = v;
        // update ui position and color
        SetHueThumbPos(currentHue);
        SetSatureThumbPos(currentSat, currentBright);

        UpdatePickColor();
    }

    public Color GetColor()
    {
        return Color.HSVToRGB(currentHue, currentSat, currentBright);
    }

}
