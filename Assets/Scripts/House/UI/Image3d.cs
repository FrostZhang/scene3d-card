using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Sprites;
using UnityEngine.U2D;

namespace UnityEngine.UI
{

    [RequireComponent(typeof(CanvasRenderer))]
    public class Image3d : MaskableGraphic, ISerializationCallbackReceiver, ILayoutElement, ICanvasRaycastFilter
    {
        public Vector4 deformation;
        public Vector2 fusion;
        public enum Type
        {
            Simple,
            Sliced,
            Tiled,
            Filled
        }

        public enum FillMethod
        {
            Horizontal,
            Vertical,
            Radial90,
            Radial180,
            Radial360
        }

        public enum OriginHorizontal
        {
            Left,
            Right
        }

        public enum OriginVertical
        {
            Bottom,
            Top
        }

        public enum Origin90
        {
            BottomLeft,
            TopLeft,
            TopRight,
            BottomRight
        }

        public enum Origin180
        {
            Bottom,
            Left,
            Top,
            Right
        }

        public enum Origin360
        {
            Bottom,
            Right,
            Top,
            Left
        }

        protected static Material s_ETC1DefaultUI = null;

        [FormerlySerializedAs("m_Frame")]
        [SerializeField]
        private Sprite m_Sprite;

        [NonSerialized]
        private Sprite m_OverrideSprite;

        [SerializeField]
        private Type m_Type;

        [SerializeField]
        private bool m_PreserveAspect;

        [SerializeField]
        private bool m_FillCenter = true;

        [SerializeField]
        private FillMethod m_FillMethod = FillMethod.Radial360;

        [Range(0f, 1f)]
        [SerializeField]
        private float m_FillAmount = 1f;

        [SerializeField]
        private bool m_FillClockwise = true;

        [SerializeField]
        private int m_FillOrigin;

        private float m_AlphaHitTestMinimumThreshold;

        private bool m_Tracked;

        [SerializeField]
        private bool m_UseSpriteMesh;

        [SerializeField]
        private float m_PixelsPerUnitMultiplier = 1f;

        private float m_CachedReferencePixelsPerUnit = 100f;

        private static readonly Vector2[] s_VertScratch = new Vector2[4];

        private static readonly Vector2[] s_UVScratch = new Vector2[4];

        private static readonly Vector3[] s_Xy = new Vector3[4];

        private static readonly Vector3[] s_Uv = new Vector3[4];

        private static List<Image3d> m_TrackedTexturelessImage3ds = new List<Image3d>();

        private static bool s_Initialized;

        public Sprite sprite
        {
            get
            {
                return m_Sprite;
            }
            set
            {
                if (m_Sprite != null)
                {
                    if (m_Sprite != value)
                    {
                        m_SkipLayoutUpdate = m_Sprite.rect.size.Equals(value ? value.rect.size : Vector2.zero);
                        m_SkipMaterialUpdate = (m_Sprite.texture == (value ? value.texture : null));
                        m_Sprite = value;
                        SetAllDirty();
                        TrackSprite();
                    }
                }
                else if (value != null)
                {
                    m_SkipLayoutUpdate = (value.rect.size == Vector2.zero);
                    m_SkipMaterialUpdate = (value.texture == null);
                    m_Sprite = value;
                    SetAllDirty();
                    TrackSprite();
                }
            }
        }

        public Sprite overrideSprite
        {
            get
            {
                return activeSprite;
            }
            set
            {
                if (m_OverrideSprite = value)
                {
                    SetAllDirty();
                    TrackSprite();
                }
            }
        }

        private Sprite activeSprite
        {
            get
            {
                if (!(m_OverrideSprite != null))
                {
                    return sprite;
                }
                return m_OverrideSprite;
            }
        }

        public Type type
        {
            get
            {
                return m_Type;
            }
            set
            {
                m_Type = value;
                SetVerticesDirty();
            }
        }

        public bool preserveAspect
        {
            get
            {
                return m_PreserveAspect;
            }
            set
            {
                m_PreserveAspect = value;
                SetVerticesDirty();
            }
        }

        public bool fillCenter
        {
            get
            {
                return m_FillCenter;
            }
            set
            {
                m_FillCenter = value;
                SetVerticesDirty();
            }
        }

        public FillMethod fillMethod
        {
            get
            {
                return m_FillMethod;
            }
            set
            {
                m_FillMethod = value;
                SetVerticesDirty();
                m_FillOrigin = 0;
            }
        }

        public float fillAmount
        {
            get
            {
                return m_FillAmount;
            }
            set
            {
                m_FillAmount = Mathf.Clamp01(value);
                SetVerticesDirty();
            }
        }

        public bool fillClockwise
        {
            get
            {
                return m_FillClockwise;
            }
            set
            {
                m_FillClockwise = value;
                SetVerticesDirty();
            }
        }

        public int fillOrigin
        {
            get
            {
                return m_FillOrigin;
            }
            set
            {
                m_FillOrigin = value;
                SetVerticesDirty();
            }
        }

        [Obsolete("eventAlphaThreshold has been deprecated. Use eventMinimumAlphaThreshold instead (UnityUpgradable) -> alphaHitTestMinimumThreshold")]
        public float eventAlphaThreshold
        {
            get
            {
                return 1f - alphaHitTestMinimumThreshold;
            }
            set
            {
                alphaHitTestMinimumThreshold = 1f - value;
            }
        }

        public float alphaHitTestMinimumThreshold
        {
            get
            {
                return m_AlphaHitTestMinimumThreshold;
            }
            set
            {
                m_AlphaHitTestMinimumThreshold = value;
            }
        }

        public bool useSpriteMesh
        {
            get
            {
                return m_UseSpriteMesh;
            }
            set
            {
                m_UseSpriteMesh = value;
                SetVerticesDirty();
            }
        }

        public static Material defaultETC1GraphicMaterial
        {
            get
            {
                if (s_ETC1DefaultUI == null)
                {
                    s_ETC1DefaultUI = Canvas.GetETC1SupportedCanvasMaterial();
                }
                return s_ETC1DefaultUI;
            }
        }

        public override Texture mainTexture
        {
            get
            {
                if (activeSprite == null)
                {
                    if (material != null && material.mainTexture != null)
                    {
                        return material.mainTexture;
                    }
                    return Graphic.s_WhiteTexture;
                }
                return activeSprite.texture;
            }
        }

        public bool hasBorder
        {
            get
            {
                if (activeSprite != null)
                {
                    return activeSprite.border.sqrMagnitude > 0f;
                }
                return false;
            }
        }

        public float pixelsPerUnitMultiplier
        {
            get
            {
                return m_PixelsPerUnitMultiplier;
            }
            set
            {
                m_PixelsPerUnitMultiplier = Mathf.Max(0.01f, value);
                SetVerticesDirty();
            }
        }

        public float pixelsPerUnit
        {
            get
            {
                float num = 100f;
                if ((bool)activeSprite)
                {
                    num = activeSprite.pixelsPerUnit;
                }
                if ((bool)base.canvas)
                {
                    m_CachedReferencePixelsPerUnit = base.canvas.referencePixelsPerUnit;
                }
                return num / m_CachedReferencePixelsPerUnit;
            }
        }

        protected float multipliedPixelsPerUnit => pixelsPerUnit * m_PixelsPerUnitMultiplier;

        public override Material material
        {
            get
            {
                if (m_Material != null)
                {
                    return m_Material;
                }
                if (Application.isPlaying && (bool)activeSprite && activeSprite.associatedAlphaSplitTexture != null)
                {
                    return defaultETC1GraphicMaterial;
                }
                return defaultMaterial;
            }
            set
            {
                base.material = value;
            }
        }

        public virtual float minWidth => 0f;

        public virtual float preferredWidth
        {
            get
            {
                if (activeSprite == null)
                {
                    return 0f;
                }
                if (type == Type.Sliced || type == Type.Tiled)
                {
                    return DataUtility.GetMinSize(activeSprite).x / pixelsPerUnit;
                }
                return activeSprite.rect.size.x / pixelsPerUnit;
            }
        }

        public virtual float flexibleWidth => -1f;

        public virtual float minHeight => 0f;

        public virtual float preferredHeight
        {
            get
            {
                if (activeSprite == null)
                {
                    return 0f;
                }
                if (type == Type.Sliced || type == Type.Tiled)
                {
                    return DataUtility.GetMinSize(activeSprite).y / pixelsPerUnit;
                }
                return activeSprite.rect.size.y / pixelsPerUnit;
            }
        }

        public virtual float flexibleHeight => -1f;

        public virtual int layoutPriority => 0;

        public void DisableSpriteOptimizations()
        {
            m_SkipLayoutUpdate = false;
            m_SkipMaterialUpdate = false;
        }

        protected Image3d()
        {
            base.useLegacyMeshGeneration = false;
        }

        public virtual void OnBeforeSerialize()
        {
        }

        public virtual void OnAfterDeserialize()
        {
            if (m_FillOrigin < 0)
            {
                m_FillOrigin = 0;
            }
            else if (m_FillMethod == FillMethod.Horizontal && m_FillOrigin > 1)
            {
                m_FillOrigin = 0;
            }
            else if (m_FillMethod == FillMethod.Vertical && m_FillOrigin > 1)
            {
                m_FillOrigin = 0;
            }
            else if (m_FillOrigin > 3)
            {
                m_FillOrigin = 0;
            }
            m_FillAmount = Mathf.Clamp(m_FillAmount, 0f, 1f);
        }

        private void PreserveSpriteAspectRatio(ref Rect rect, Vector2 spriteSize)
        {
            float num = spriteSize.x / spriteSize.y;
            float num2 = rect.width / rect.height;
            if (num > num2)
            {
                float height = rect.height;
                rect.height = rect.width * (1f / num);
                rect.y += (height - rect.height) * base.rectTransform.pivot.y;
            }
            else
            {
                float width = rect.width;
                rect.width = rect.height * num;
                rect.x += (width - rect.width) * base.rectTransform.pivot.x;
            }
        }

        private Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
        {
            Vector4 vector = (activeSprite == null) ? Vector4.zero : DataUtility.GetPadding(activeSprite);
            Vector2 spriteSize = (activeSprite == null) ? Vector2.zero : new Vector2(activeSprite.rect.width, activeSprite.rect.height);
            Rect rect = GetPixelAdjustedRect();
            int num = Mathf.RoundToInt(spriteSize.x);
            int num2 = Mathf.RoundToInt(spriteSize.y);
            Vector4 vector2 = new Vector4(vector.x / (float)num, vector.y / (float)num2, ((float)num - vector.z) / (float)num, ((float)num2 - vector.w) / (float)num2);
            if (shouldPreserveAspect && spriteSize.sqrMagnitude > 0f)
            {
                PreserveSpriteAspectRatio(ref rect, spriteSize);
            }
            return new Vector4(rect.x + rect.width * vector2.x, rect.y + rect.height * vector2.y, rect.x + rect.width * vector2.z, rect.y + rect.height * vector2.w);
        }

        public override void SetNativeSize()
        {
            if (activeSprite != null)
            {
                float x = activeSprite.rect.width / pixelsPerUnit;
                float y = activeSprite.rect.height / pixelsPerUnit;
                base.rectTransform.anchorMax = base.rectTransform.anchorMin;
                base.rectTransform.sizeDelta = new Vector2(x, y);
                SetAllDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (activeSprite == null)
            {
                base.OnPopulateMesh(toFill);
                return;
            }
            switch (type)
            {
                case Type.Simple:
                    if (!useSpriteMesh)
                    {
                        GenerateSimpleSprite(toFill, m_PreserveAspect);
                    }
                    else
                    {
                        GenerateSprite(toFill, m_PreserveAspect);
                    }
                    break;
                case Type.Sliced:
                    GenerateSlicedSprite(toFill);
                    break;
                case Type.Tiled:
                    GenerateTiledSprite(toFill);
                    break;
                case Type.Filled:
                    GenerateFilledSprite(toFill, m_PreserveAspect);
                    break;
            }
        }

        private void TrackSprite()
        {
            if (activeSprite != null && activeSprite.texture == null)
            {
                TrackImage3d(this);
                m_Tracked = true;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            TrackSprite();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (m_Tracked)
            {
                UnTrackImage3d(this);
            }
        }

        protected override void UpdateMaterial()
        {
            base.UpdateMaterial();
            if (activeSprite == null)
            {
                base.canvasRenderer.SetAlphaTexture(null);
                return;
            }
            Texture2D associatedAlphaSplitTexture = activeSprite.associatedAlphaSplitTexture;
            if (associatedAlphaSplitTexture != null)
            {
                base.canvasRenderer.SetAlphaTexture(associatedAlphaSplitTexture);
            }
        }

        protected override void OnCanvasHierarchyChanged()
        {
            base.OnCanvasHierarchyChanged();
            if (base.canvas == null)
            {
                m_CachedReferencePixelsPerUnit = 100f;
            }
            else if (base.canvas.referencePixelsPerUnit != m_CachedReferencePixelsPerUnit)
            {
                m_CachedReferencePixelsPerUnit = base.canvas.referencePixelsPerUnit;
                if (type == Type.Sliced || type == Type.Tiled)
                {
                    SetVerticesDirty();
                    SetLayoutDirty();
                }
            }
        }

        private void GenerateSimpleSprite(VertexHelper vh, bool lPreserveAspect)
        {
            Vector4 drawingDimensions = GetDrawingDimensions(lPreserveAspect);
            Vector4 vector = (activeSprite != null) ? DataUtility.GetOuterUV(activeSprite) : Vector4.zero;
            Color color = this.color;
            vh.Clear();
            vh.AddVert(new Vector3(drawingDimensions.x + fusion.x, drawingDimensions.y + deformation.x), color, new Vector2(vector.x, vector.y));
            vh.AddVert(new Vector3(drawingDimensions.x + fusion.x, drawingDimensions.w + deformation.y), color, new Vector2(vector.x, vector.w));
            vh.AddVert(new Vector3(drawingDimensions.z + fusion.y, drawingDimensions.w + deformation.z), color, new Vector2(vector.z, vector.w));
            vh.AddVert(new Vector3(drawingDimensions.z + fusion.y, drawingDimensions.y + deformation.w), color, new Vector2(vector.z, vector.y));
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

        private void GenerateSprite(VertexHelper vh, bool lPreserveAspect)
        {
            Vector2 vector = new Vector2(activeSprite.rect.width, activeSprite.rect.height);
            Vector2 b = activeSprite.pivot / vector;
            Vector2 pivot = base.rectTransform.pivot;
            Rect rect = GetPixelAdjustedRect();
            if (lPreserveAspect & (vector.sqrMagnitude > 0f))
            {
                PreserveSpriteAspectRatio(ref rect, vector);
            }
            Vector2 b2 = new Vector2(rect.width, rect.height);
            Vector3 size = activeSprite.bounds.size;
            Vector2 vector2 = (pivot - b) * b2;
            Color color = this.color;
            vh.Clear();
            Vector2[] vertices = activeSprite.vertices;
            Vector2[] uv = activeSprite.uv;
            for (int i = 0; i < vertices.Length; i++)
            {
                vh.AddVert(new Vector3(vertices[i].x / size.x * b2.x - vector2.x, vertices[i].y / size.y * b2.y - vector2.y), color, new Vector2(uv[i].x, uv[i].y));
            }
            ushort[] triangles = activeSprite.triangles;
            for (int j = 0; j < triangles.Length; j += 3)
            {
                vh.AddTriangle(triangles[j], triangles[j + 1], triangles[j + 2]);
            }
        }

        private void GenerateSlicedSprite(VertexHelper toFill)
        {
            if (!hasBorder)
            {
                GenerateSimpleSprite(toFill, lPreserveAspect: false);
                return;
            }
            Vector4 vector;
            Vector4 vector2;
            Vector4 vector3;
            Vector4 a;
            if (activeSprite != null)
            {
                vector = DataUtility.GetOuterUV(activeSprite);
                vector2 = DataUtility.GetInnerUV(activeSprite);
                vector3 = DataUtility.GetPadding(activeSprite);
                a = activeSprite.border;
            }
            else
            {
                vector = Vector4.zero;
                vector2 = Vector4.zero;
                vector3 = Vector4.zero;
                a = Vector4.zero;
            }
            Rect pixelAdjustedRect = GetPixelAdjustedRect();
            Vector4 adjustedBorders = GetAdjustedBorders(a / multipliedPixelsPerUnit, pixelAdjustedRect);
            vector3 /= multipliedPixelsPerUnit;
            s_VertScratch[0] = new Vector2(vector3.x, vector3.y);
            s_VertScratch[3] = new Vector2(pixelAdjustedRect.width - vector3.z, pixelAdjustedRect.height - vector3.w);
            s_VertScratch[1].x = adjustedBorders.x;
            s_VertScratch[1].y = adjustedBorders.y;
            s_VertScratch[2].x = pixelAdjustedRect.width - adjustedBorders.z;
            s_VertScratch[2].y = pixelAdjustedRect.height - adjustedBorders.w;
            for (int i = 0; i < 4; i++)
            {
                s_VertScratch[i].x += pixelAdjustedRect.x;
                s_VertScratch[i].y += pixelAdjustedRect.y;
            }
            s_UVScratch[0] = new Vector2(vector.x, vector.y);
            s_UVScratch[1] = new Vector2(vector2.x, vector2.y);
            s_UVScratch[2] = new Vector2(vector2.z, vector2.w);
            s_UVScratch[3] = new Vector2(vector.z, vector.w);
            toFill.Clear();
            for (int j = 0; j < 3; j++)
            {
                int num = j + 1;
                for (int k = 0; k < 3; k++)
                {
                    if (m_FillCenter || j != 1 || k != 1)
                    {
                        int num2 = k + 1;
                        AddQuad(toFill, new Vector2(s_VertScratch[j].x, s_VertScratch[k].y), new Vector2(s_VertScratch[num].x, s_VertScratch[num2].y), color, new Vector2(s_UVScratch[j].x, s_UVScratch[k].y), new Vector2(s_UVScratch[num].x, s_UVScratch[num2].y));
                    }
                }
            }
        }

        private void GenerateTiledSprite(VertexHelper toFill)
        {
            Vector4 vector;
            Vector4 vector2;
            Vector2 vector3;
            Vector4 a;
            if (activeSprite != null)
            {
                vector = DataUtility.GetOuterUV(activeSprite);
                vector2 = DataUtility.GetInnerUV(activeSprite);
                a = activeSprite.border;
                vector3 = activeSprite.rect.size;
            }
            else
            {
                vector = Vector4.zero;
                vector2 = Vector4.zero;
                a = Vector4.zero;
                vector3 = Vector2.one * 100f;
            }
            Rect pixelAdjustedRect = GetPixelAdjustedRect();
            float num = (vector3.x - a.x - a.z) / multipliedPixelsPerUnit;
            float num2 = (vector3.y - a.y - a.w) / multipliedPixelsPerUnit;
            a = GetAdjustedBorders(a / multipliedPixelsPerUnit, pixelAdjustedRect);
            Vector2 vector4 = new Vector2(vector2.x, vector2.y);
            Vector2 vector5 = new Vector2(vector2.z, vector2.w);
            float x = a.x;
            float num3 = pixelAdjustedRect.width - a.z;
            float y = a.y;
            float num4 = pixelAdjustedRect.height - a.w;
            toFill.Clear();
            Vector2 uvMax = vector5;
            if (num <= 0f)
            {
                num = num3 - x;
            }
            if (num2 <= 0f)
            {
                num2 = num4 - y;
            }
            if (activeSprite != null && (hasBorder || activeSprite.packed || activeSprite.texture.wrapMode != 0))
            {
                long num5 = 0L;
                long num6 = 0L;
                if (m_FillCenter)
                {
                    num5 = (long)Math.Ceiling((num3 - x) / num);
                    num6 = (long)Math.Ceiling((num4 - y) / num2);
                    double num7 = 0.0;
                    num7 = ((!hasBorder) ? ((double)(num5 * num6) * 4.0) : (((double)num5 + 2.0) * ((double)num6 + 2.0) * 4.0));
                    if (num7 > 65000.0)
                    {
                        Debug.LogError("Too many sprite tiles on Image3d \"" + base.name + "\". The tile size will be increased. To remove the limit on the number of tiles, set the Wrap mode to Repeat in the Image3d Import Settings", this);
                        double num8 = (!hasBorder) ? ((double)num5 / (double)num6) : (((double)num5 + 2.0) / ((double)num6 + 2.0));
                        double num9 = Math.Sqrt(16250.0 / num8);
                        double num10 = num9 * num8;
                        if (hasBorder)
                        {
                            num9 -= 2.0;
                            num10 -= 2.0;
                        }
                        num5 = (long)Math.Floor(num9);
                        num6 = (long)Math.Floor(num10);
                        num = (num3 - x) / (float)num5;
                        num2 = (num4 - y) / (float)num6;
                    }
                }
                else if (hasBorder)
                {
                    num5 = (long)Math.Ceiling((num3 - x) / num);
                    num6 = (long)Math.Ceiling((num4 - y) / num2);
                    if (((double)(num6 + num5) + 2.0) * 2.0 * 4.0 > 65000.0)
                    {
                        Debug.LogError("Too many sprite tiles on Image3d \"" + base.name + "\". The tile size will be increased. To remove the limit on the number of tiles, set the Wrap mode to Repeat in the Image3d Import Settings", this);
                        double num11 = (double)num5 / (double)num6;
                        double num12 = (16250.0 - 4.0) / (2.0 * (1.0 + num11));
                        double d = num12 * num11;
                        num5 = (long)Math.Floor(num12);
                        num6 = (long)Math.Floor(d);
                        num = (num3 - x) / (float)num5;
                        num2 = (num4 - y) / (float)num6;
                    }
                }
                else
                {
                    num6 = (num5 = 0L);
                }
                if (m_FillCenter)
                {
                    for (long num13 = 0L; num13 < num6; num13++)
                    {
                        float num14 = y + (float)num13 * num2;
                        float num15 = y + (float)(num13 + 1) * num2;
                        if (num15 > num4)
                        {
                            uvMax.y = vector4.y + (vector5.y - vector4.y) * (num4 - num14) / (num15 - num14);
                            num15 = num4;
                        }
                        uvMax.x = vector5.x;
                        for (long num16 = 0L; num16 < num5; num16++)
                        {
                            float num17 = x + (float)num16 * num;
                            float num18 = x + (float)(num16 + 1) * num;
                            if (num18 > num3)
                            {
                                uvMax.x = vector4.x + (vector5.x - vector4.x) * (num3 - num17) / (num18 - num17);
                                num18 = num3;
                            }
                            AddQuad(toFill, new Vector2(num17, num14) + pixelAdjustedRect.position, new Vector2(num18, num15) + pixelAdjustedRect.position, color, vector4, uvMax);
                        }
                    }
                }
                if (!hasBorder)
                {
                    return;
                }
                uvMax = vector5;
                for (long num19 = 0L; num19 < num6; num19++)
                {
                    float num20 = y + (float)num19 * num2;
                    float num21 = y + (float)(num19 + 1) * num2;
                    if (num21 > num4)
                    {
                        uvMax.y = vector4.y + (vector5.y - vector4.y) * (num4 - num20) / (num21 - num20);
                        num21 = num4;
                    }
                    AddQuad(toFill, new Vector2(0f, num20) + pixelAdjustedRect.position, new Vector2(x, num21) + pixelAdjustedRect.position, color, new Vector2(vector.x, vector4.y), new Vector2(vector4.x, uvMax.y));
                    AddQuad(toFill, new Vector2(num3, num20) + pixelAdjustedRect.position, new Vector2(pixelAdjustedRect.width, num21) + pixelAdjustedRect.position, color, new Vector2(vector5.x, vector4.y), new Vector2(vector.z, uvMax.y));
                }
                uvMax = vector5;
                for (long num22 = 0L; num22 < num5; num22++)
                {
                    float num23 = x + (float)num22 * num;
                    float num24 = x + (float)(num22 + 1) * num;
                    if (num24 > num3)
                    {
                        uvMax.x = vector4.x + (vector5.x - vector4.x) * (num3 - num23) / (num24 - num23);
                        num24 = num3;
                    }
                    AddQuad(toFill, new Vector2(num23, 0f) + pixelAdjustedRect.position, new Vector2(num24, y) + pixelAdjustedRect.position, color, new Vector2(vector4.x, vector.y), new Vector2(uvMax.x, vector4.y));
                    AddQuad(toFill, new Vector2(num23, num4) + pixelAdjustedRect.position, new Vector2(num24, pixelAdjustedRect.height) + pixelAdjustedRect.position, color, new Vector2(vector4.x, vector5.y), new Vector2(uvMax.x, vector.w));
                }
                AddQuad(toFill, new Vector2(0f, 0f) + pixelAdjustedRect.position, new Vector2(x, y) + pixelAdjustedRect.position, color, new Vector2(vector.x, vector.y), new Vector2(vector4.x, vector4.y));
                AddQuad(toFill, new Vector2(num3, 0f) + pixelAdjustedRect.position, new Vector2(pixelAdjustedRect.width, y) + pixelAdjustedRect.position, color, new Vector2(vector5.x, vector.y), new Vector2(vector.z, vector4.y));
                AddQuad(toFill, new Vector2(0f, num4) + pixelAdjustedRect.position, new Vector2(x, pixelAdjustedRect.height) + pixelAdjustedRect.position, color, new Vector2(vector.x, vector5.y), new Vector2(vector4.x, vector.w));
                AddQuad(toFill, new Vector2(num3, num4) + pixelAdjustedRect.position, new Vector2(pixelAdjustedRect.width, pixelAdjustedRect.height) + pixelAdjustedRect.position, color, new Vector2(vector5.x, vector5.y), new Vector2(vector.z, vector.w));
            }
            else
            {
                Vector2 b = new Vector2((num3 - x) / num, (num4 - y) / num2);
                if (m_FillCenter)
                {
                    AddQuad(toFill, new Vector2(x, y) + pixelAdjustedRect.position, new Vector2(num3, num4) + pixelAdjustedRect.position, color, Vector2.Scale(vector4, b), Vector2.Scale(vector5, b));
                }
            }
        }

        private static void AddQuad(VertexHelper vertexHelper, Vector3[] quadPositions, Color32 color, Vector3[] quadUVs)
        {
            int currentVertCount = vertexHelper.currentVertCount;
            for (int i = 0; i < 4; i++)
            {
                vertexHelper.AddVert(quadPositions[i], color, quadUVs[i]);
            }
            vertexHelper.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
            vertexHelper.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
        }

        private static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax)
        {
            int currentVertCount = vertexHelper.currentVertCount;
            vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0f), color, new Vector2(uvMin.x, uvMin.y));
            vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0f), color, new Vector2(uvMin.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0f), color, new Vector2(uvMax.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0f), color, new Vector2(uvMax.x, uvMin.y));
            vertexHelper.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
            vertexHelper.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
        }

        private Vector4 GetAdjustedBorders(Vector4 border, Rect adjustedRect)
        {
            Rect rect = base.rectTransform.rect;
            for (int i = 0; i <= 1; i++)
            {
                if (rect.size[i] != 0f)
                {
                    float num = adjustedRect.size[i] / rect.size[i];
                    border[i] *= num;
                    border[i + 2] *= num;
                }
                float num2 = border[i] + border[i + 2];
                if (adjustedRect.size[i] < num2 && num2 != 0f)
                {
                    float num = adjustedRect.size[i] / num2;
                    border[i] *= num;
                    border[i + 2] *= num;
                }
            }
            return border;
        }

        private void GenerateFilledSprite(VertexHelper toFill, bool preserveAspect)
        {
            //IL_003c: Unknown result type (might be due to invalid IL or missing references)
            //IL_0041: Unknown result type (might be due to invalid IL or missing references)
            toFill.Clear();
            if (m_FillAmount < 0.001f)
            {
                return;
            }
            Vector4 drawingDimensions = GetDrawingDimensions(preserveAspect);
            Vector4 obj = (activeSprite != null) ? DataUtility.GetOuterUV(activeSprite) : Vector4.zero;
            UIVertex simpleVert = UIVertex.simpleVert;
            simpleVert.color = color;
            float num = obj.x;
            float num2 = obj.y;
            float num3 = obj.z;
            float num4 = obj.w;
            if (m_FillMethod == FillMethod.Horizontal || m_FillMethod == FillMethod.Vertical)
            {
                if (fillMethod == FillMethod.Horizontal)
                {
                    float num5 = (num3 - num) * m_FillAmount;
                    if (m_FillOrigin == 1)
                    {
                        drawingDimensions.x = drawingDimensions.z - (drawingDimensions.z - drawingDimensions.x) * m_FillAmount;
                        num = num3 - num5;
                    }
                    else
                    {
                        drawingDimensions.z = drawingDimensions.x + (drawingDimensions.z - drawingDimensions.x) * m_FillAmount;
                        num3 = num + num5;
                    }
                }
                else if (fillMethod == FillMethod.Vertical)
                {
                    float num6 = (num4 - num2) * m_FillAmount;
                    if (m_FillOrigin == 1)
                    {
                        drawingDimensions.y = drawingDimensions.w - (drawingDimensions.w - drawingDimensions.y) * m_FillAmount;
                        num2 = num4 - num6;
                    }
                    else
                    {
                        drawingDimensions.w = drawingDimensions.y + (drawingDimensions.w - drawingDimensions.y) * m_FillAmount;
                        num4 = num2 + num6;
                    }
                }
            }
            s_Xy[0] = new Vector2(drawingDimensions.x, drawingDimensions.y);
            s_Xy[1] = new Vector2(drawingDimensions.x, drawingDimensions.w);
            s_Xy[2] = new Vector2(drawingDimensions.z, drawingDimensions.w);
            s_Xy[3] = new Vector2(drawingDimensions.z, drawingDimensions.y);
            s_Uv[0] = new Vector2(num, num2);
            s_Uv[1] = new Vector2(num, num4);
            s_Uv[2] = new Vector2(num3, num4);
            s_Uv[3] = new Vector2(num3, num2);
            if (m_FillAmount < 1f && m_FillMethod != 0 && m_FillMethod != FillMethod.Vertical)
            {
                if (fillMethod == FillMethod.Radial90)
                {
                    if (RadialCut(s_Xy, s_Uv, m_FillAmount, m_FillClockwise, m_FillOrigin))
                    {
                        AddQuad(toFill, s_Xy, color, s_Uv);
                    }
                }
                else if (fillMethod == FillMethod.Radial180)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        int num7 = (m_FillOrigin > 1) ? 1 : 0;
                        float t;
                        float t2;
                        float t3;
                        float t4;
                        if (m_FillOrigin == 0 || m_FillOrigin == 2)
                        {
                            t = 0f;
                            t2 = 1f;
                            if (i == num7)
                            {
                                t3 = 0f;
                                t4 = 0.5f;
                            }
                            else
                            {
                                t3 = 0.5f;
                                t4 = 1f;
                            }
                        }
                        else
                        {
                            t3 = 0f;
                            t4 = 1f;
                            if (i == num7)
                            {
                                t = 0.5f;
                                t2 = 1f;
                            }
                            else
                            {
                                t = 0f;
                                t2 = 0.5f;
                            }
                        }
                        s_Xy[0].x = Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, t3);
                        s_Xy[1].x = s_Xy[0].x;
                        s_Xy[2].x = Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, t4);
                        s_Xy[3].x = s_Xy[2].x;
                        s_Xy[0].y = Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, t);
                        s_Xy[1].y = Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, t2);
                        s_Xy[2].y = s_Xy[1].y;
                        s_Xy[3].y = s_Xy[0].y;
                        s_Uv[0].x = Mathf.Lerp(num, num3, t3);
                        s_Uv[1].x = s_Uv[0].x;
                        s_Uv[2].x = Mathf.Lerp(num, num3, t4);
                        s_Uv[3].x = s_Uv[2].x;
                        s_Uv[0].y = Mathf.Lerp(num2, num4, t);
                        s_Uv[1].y = Mathf.Lerp(num2, num4, t2);
                        s_Uv[2].y = s_Uv[1].y;
                        s_Uv[3].y = s_Uv[0].y;
                        float value = m_FillClockwise ? (fillAmount * 2f - (float)i) : (m_FillAmount * 2f - (float)(1 - i));
                        if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(value), m_FillClockwise, (i + m_FillOrigin + 3) % 4))
                        {
                            AddQuad(toFill, s_Xy, color, s_Uv);
                        }
                    }
                }
                else
                {
                    if (fillMethod != FillMethod.Radial360)
                    {
                        return;
                    }
                    for (int j = 0; j < 4; j++)
                    {
                        float t5;
                        float t6;
                        if (j < 2)
                        {
                            t5 = 0f;
                            t6 = 0.5f;
                        }
                        else
                        {
                            t5 = 0.5f;
                            t6 = 1f;
                        }
                        float t7;
                        float t8;
                        if (j == 0 || j == 3)
                        {
                            t7 = 0f;
                            t8 = 0.5f;
                        }
                        else
                        {
                            t7 = 0.5f;
                            t8 = 1f;
                        }
                        s_Xy[0].x = Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, t5);
                        s_Xy[1].x = s_Xy[0].x;
                        s_Xy[2].x = Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, t6);
                        s_Xy[3].x = s_Xy[2].x;
                        s_Xy[0].y = Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, t7);
                        s_Xy[1].y = Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, t8);
                        s_Xy[2].y = s_Xy[1].y;
                        s_Xy[3].y = s_Xy[0].y;
                        s_Uv[0].x = Mathf.Lerp(num, num3, t5);
                        s_Uv[1].x = s_Uv[0].x;
                        s_Uv[2].x = Mathf.Lerp(num, num3, t6);
                        s_Uv[3].x = s_Uv[2].x;
                        s_Uv[0].y = Mathf.Lerp(num2, num4, t7);
                        s_Uv[1].y = Mathf.Lerp(num2, num4, t8);
                        s_Uv[2].y = s_Uv[1].y;
                        s_Uv[3].y = s_Uv[0].y;
                        float value2 = m_FillClockwise ? (m_FillAmount * 4f - (float)((j + m_FillOrigin) % 4)) : (m_FillAmount * 4f - (float)(3 - (j + m_FillOrigin) % 4));
                        if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(value2), m_FillClockwise, (j + 2) % 4))
                        {
                            AddQuad(toFill, s_Xy, color, s_Uv);
                        }
                    }
                }
            }
            else
            {
                AddQuad(toFill, s_Xy, color, s_Uv);
            }
        }

        private static bool RadialCut(Vector3[] xy, Vector3[] uv, float fill, bool invert, int corner)
        {
            if (fill < 0.001f)
            {
                return false;
            }
            if ((corner & 1) == 1)
            {
                invert = !invert;
            }
            if (!invert && fill > 0.999f)
            {
                return true;
            }
            float num = Mathf.Clamp01(fill);
            if (invert)
            {
                num = 1f - num;
            }
            num *= (float)Math.PI / 2f;
            float cos = Mathf.Cos(num);
            float sin = Mathf.Sin(num);
            RadialCut(xy, cos, sin, invert, corner);
            RadialCut(uv, cos, sin, invert, corner);
            return true;
        }

        private static void RadialCut(Vector3[] xy, float cos, float sin, bool invert, int corner)
        {
            int num = (corner + 1) % 4;
            int num2 = (corner + 2) % 4;
            int num3 = (corner + 3) % 4;
            if ((corner & 1) == 1)
            {
                if (sin > cos)
                {
                    cos /= sin;
                    sin = 1f;
                    if (invert)
                    {
                        xy[num].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
                        xy[num2].x = xy[num].x;
                    }
                }
                else if (cos > sin)
                {
                    sin /= cos;
                    cos = 1f;
                    if (!invert)
                    {
                        xy[num2].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
                        xy[num3].y = xy[num2].y;
                    }
                }
                else
                {
                    cos = 1f;
                    sin = 1f;
                }
                if (!invert)
                {
                    xy[num3].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
                }
                else
                {
                    xy[num].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
                }
                return;
            }
            if (cos > sin)
            {
                sin /= cos;
                cos = 1f;
                if (!invert)
                {
                    xy[num].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
                    xy[num2].y = xy[num].y;
                }
            }
            else if (sin > cos)
            {
                cos /= sin;
                sin = 1f;
                if (invert)
                {
                    xy[num2].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
                    xy[num3].x = xy[num2].x;
                }
            }
            else
            {
                cos = 1f;
                sin = 1f;
            }
            if (invert)
            {
                xy[num3].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
            }
            else
            {
                xy[num].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
            }
        }

        public virtual void CalculateLayoutInputHorizontal()
        {
        }

        public virtual void CalculateLayoutInputVertical()
        {
        }

        public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (alphaHitTestMinimumThreshold <= 0f)
            {
                return true;
            }
            if (alphaHitTestMinimumThreshold > 1f)
            {
                return false;
            }
            if (activeSprite == null)
            {
                return true;
            }
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(base.rectTransform, screenPoint, eventCamera, out Vector2 localPoint))
            {
                return false;
            }
            Rect pixelAdjustedRect = GetPixelAdjustedRect();
            localPoint.x += base.rectTransform.pivot.x * pixelAdjustedRect.width;
            localPoint.y += base.rectTransform.pivot.y * pixelAdjustedRect.height;
            localPoint = MapCoordinate(localPoint, pixelAdjustedRect);
            Rect textureRect = activeSprite.textureRect;
            float x = (textureRect.x + localPoint.x) / (float)activeSprite.texture.width;
            float y = (textureRect.y + localPoint.y) / (float)activeSprite.texture.height;
            try
            {
                return activeSprite.texture.GetPixelBilinear(x, y).a >= alphaHitTestMinimumThreshold;
            }
            catch (UnityException ex)
            {
                Debug.LogError("Using alphaHitTestMinimumThreshold greater than 0 on Image3d whose sprite texture cannot be read. " + ex.Message + " Also make sure to disable sprite packing for this sprite.", this);
                return true;
            }
        }

        private Vector2 MapCoordinate(Vector2 local, Rect rect)
        {
            Rect rect2 = activeSprite.rect;
            if (type == Type.Simple || type == Type.Filled)
            {
                return new Vector2(local.x * rect2.width / rect.width, local.y * rect2.height / rect.height);
            }
            Vector4 border = activeSprite.border;
            Vector4 adjustedBorders = GetAdjustedBorders(border / pixelsPerUnit, rect);
            for (int i = 0; i < 2; i++)
            {
                if (!(local[i] <= adjustedBorders[i]))
                {
                    if (rect.size[i] - local[i] <= adjustedBorders[i + 2])
                    {
                        local[i] -= rect.size[i] - rect2.size[i];
                    }
                    else if (type == Type.Sliced)
                    {
                        float t = Mathf.InverseLerp(adjustedBorders[i], rect.size[i] - adjustedBorders[i + 2], local[i]);
                        local[i] = Mathf.Lerp(border[i], rect2.size[i] - border[i + 2], t);
                    }
                    else
                    {
                        local[i] -= adjustedBorders[i];
                        local[i] = Mathf.Repeat(local[i], rect2.size[i] - border[i] - border[i + 2]);
                        local[i] += border[i];
                    }
                }
            }
            return local;
        }

        private static void RebuildImage3d(SpriteAtlas spriteAtlas)
        {
            for (int num = m_TrackedTexturelessImage3ds.Count - 1; num >= 0; num--)
            {
                Image3d Image3d = m_TrackedTexturelessImage3ds[num];
                if (null != Image3d.activeSprite && spriteAtlas.CanBindTo(Image3d.activeSprite))
                {
                    Image3d.SetAllDirty();
                    m_TrackedTexturelessImage3ds.RemoveAt(num);
                }
            }
        }

        private static void TrackImage3d(Image3d g)
        {
            if (!s_Initialized)
            {
                SpriteAtlasManager.atlasRegistered += ((Action<SpriteAtlas>)RebuildImage3d);
                s_Initialized = true;
            }
            m_TrackedTexturelessImage3ds.Add(g);
        }

        private static void UnTrackImage3d(Image3d g)
        {
            m_TrackedTexturelessImage3ds.Remove(g);
        }

        protected override void OnDidApplyAnimationProperties()
        {
            SetMaterialDirty();
            SetVerticesDirty();
        }
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            m_PixelsPerUnitMultiplier = Mathf.Max(0.01f, m_PixelsPerUnitMultiplier);
        }
#endif
    }
}
