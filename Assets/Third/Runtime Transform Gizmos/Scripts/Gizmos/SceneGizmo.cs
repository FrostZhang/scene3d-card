using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RTEditor
{
    public class SceneGizmo : MonoSingletonBase<SceneGizmo>
    {
        #region Private Enums
        private enum GizmoCompTransition
        {
            FadeIn = 0,
            FadeOut,
        }
        #endregion

        #region Private Classes and Structs
        private class GizmoCompAlphaFadeInfo
        {
            #region Private Variables
            private GizmoCompTransition _transition;
            private float _elapsedTime;
            private bool _isActive;
            private float _srcAlpha;
            private float _destAlpha;
            #endregion

            #region Public Properties
            public bool IsActive { get { return _isActive; } }
            public float ElapsedTime { get { return _elapsedTime; } set { _elapsedTime = value; } }
            public float SrcAlpha { get { return _srcAlpha; } }
            public float DestAlpha { get { return _destAlpha; } }
            public GizmoCompTransition Transition { get { return _transition; } }
            #endregion

            #region Constructors
            public void ChangeTransition(GizmoCompTransition newTransition, float srcAlpha)
            {
                if (_transition != newTransition)
                {
                    _transition = newTransition;
                    _srcAlpha = srcAlpha;
                    _destAlpha = newTransition == GizmoCompTransition.FadeIn ? 1.0f : 0.0f;
                    _elapsedTime = 0.0f;
                    _isActive = true;
                }
            }

            public void Stop()
            {
                _isActive = false;
            }
            #endregion
        }
        #endregion

        #region Private Variables
        private Camera _gizmoCamera;
        private Transform _gizmoCameraTransform;
        private Transform _gizmoTransform;

        [SerializeField]
        private SceneGizmoCorner _corner = SceneGizmoCorner.TopRight;
        [SerializeField]
        private Color _negativeAxisColor = DefaultNegativeAxisColor;
        [SerializeField]
        private Color _xAxisColor = DefaultXAxisColor;
        [SerializeField]
        private Color _yAxisColor = DefaultYAxisColor;
        [SerializeField]
        private Color _zAxisColor = DefaultZAxisColor;
        [SerializeField]
        private Color _cubeColor = DefaultCubeColor;
        [SerializeField]
        private Color _hoveredComponentColor = DefaultHoveredComponentColor;
        [SerializeField]
        private float _cameraAlignDuration = 0.3f;

        private Texture2D[] _axisLabelTextures = new Texture2D[3];

        private float _screenSize = DefaultScreenSize;

        private int _hoveredComponent = -1;
        private Color[] _componentColors = new Color[Enum.GetValues(typeof(SceneGizmoComponent)).Length];
        private float[] _componentAlphas = new float[Enum.GetValues(typeof(SceneGizmoComponent)).Length];
        private GizmoCompAlphaFadeInfo[] _componentAlphaFadeInfo = new GizmoCompAlphaFadeInfo[Enum.GetValues(typeof(SceneGizmoComponent)).Length];
        #endregion

        #region Public Static Properties
        public static float MinScreenSize { get { return 1e-1f; } }
        public static float MaxScreenSize { get { return 200.0f; } }
        public static float MinDuration { get { return 0.001f; } }
        public static Color DefaultXAxisColor { get { return new Color(219.0f / 255.0f, 62.0f / 255.0f, 29.0f / 255.0f, 1.0f); } }
        public static Color DefaultYAxisColor { get { return new Color(154.0f / 255.0f, 243.0f / 255.0f, 72.0f / 255.0f, 1.0f); } }
        public static Color DefaultZAxisColor { get { return new Color(58.0f / 255.0f, 122.0f / 255.0f, 248.0f / 255.0f, 1.0f); } }
        public static Color DefaultHoveredComponentColor { get { return new Color(246.0f / 255.0f, 242.0f / 255.0f, 50.0f / 255.0f, 1.0f); } }
        public static Color DefaultCubeColor { get { return new Color(204.0f / 255.0f, 204.0f / 255.0f, 204.0f / 255.0f, 237.0f / 255.0f); } }
        public static Color DefaultNegativeAxisColor { get { return DefaultCubeColor; } }
        public static float DefaultScreenSize { get { return 100.0f; } }
        public static int DefaultAxisLabelCharSize { get { return 2; } }
        #endregion

        #region Public Properties
        public SceneGizmoCorner Corner { get { return _corner; } set { _corner = value; } }
        public float CameraAlignDuration { get { return _cameraAlignDuration; } set { _cameraAlignDuration = Mathf.Max(value, MinDuration); } }
        public Color NegativeAxisColor { get { return _negativeAxisColor; } set { _negativeAxisColor = value; } }
        public Color XAxisColor { get { return _xAxisColor; } set { _xAxisColor = value; } }
        public Color YAxisColor { get { return _yAxisColor; } set { _yAxisColor = value; } }
        public Color ZAxisColor { get { return _zAxisColor; } set { _zAxisColor = value; } }
        public Color HoveredComponentColor { get { return _hoveredComponentColor; } set { _hoveredComponentColor = value; } }
        public Color CubeColor { get { return _cubeColor; } set { _cubeColor = value; } }
        #endregion

        #region Public Methods
        public bool IsHovered()
        {
            return DetectHoveredComponent() != -1;
        }
        #endregion

        #region Private Methods
        private void Awake()
        {
            _gizmoTransform = transform;
        }

        private void Start()
        {
            CreateSceneGizmoCamera();

            _axisLabelTextures[0] = Resources.Load("Textures/XAxisLabel") as Texture2D;
            _axisLabelTextures[1] = Resources.Load("Textures/YAxisLabel") as Texture2D;
            _axisLabelTextures[2] = Resources.Load("Textures/ZAxisLabel") as Texture2D;

            int axisCount = Enum.GetValues(typeof(SceneGizmoComponent)).Length;
            for (int axisIndex = 0; axisIndex < axisCount; ++axisIndex)
            {
                _componentAlphaFadeInfo[axisIndex] = new GizmoCompAlphaFadeInfo();
                _componentAlphas[axisIndex] = 1.0f;
            }

            UpdateGizmoCamera();
            StartCoroutine(DoComponentAlphaTransitions());
        }

        private void Update()
        {
            UpdateGizmoCamera();

            _gizmoTransform.rotation = CalculateGizmoRotation();
            _gizmoTransform.gameObject.SetAbsoluteScale(Vector3.one);
            _gizmoTransform.position = CalculateCubePosition();

            _hoveredComponent = DetectHoveredComponent();
            EstablishComponentColors();

            if (InputDevice.Instance.WasPressedInCurrentFrame(0)) OnFirstInputDeviceBtnDown();
        }

        private void OnGUI()
        {
            //Rect gizmoCamRect = _gizmoCamera.pixelRect;
            //Rect labelrect = new Rect(gizmoCamRect.xMin + 35.0f, Screen.height - gizmoCamRect.yMin - 12.0f, gizmoCamRect.width, gizmoCamRect.height);
            //GUI.Label(labelrect, _gizmoCamera.orthographic ? "Ortho" : "Persp");
        }

        private void OnFirstInputDeviceBtnDown()
        {
            if (_hoveredComponent == (int)SceneGizmoComponent.Cube)
            {
                Camera editorCamera = EditorCamera.Instance.Camera;
                EditorCamera.Instance.SetOrtho(!editorCamera.orthographic);
                _gizmoCamera.orthographic = editorCamera.orthographic;
            }
            if (_hoveredComponent == (int)SceneGizmoComponent.PositiveX) EditorCamera.Instance.AlignLookWithWorldAxis(Axis.X, false, _cameraAlignDuration);
            else if (_hoveredComponent == (int)SceneGizmoComponent.NegativeX) EditorCamera.Instance.AlignLookWithWorldAxis(Axis.X, true, _cameraAlignDuration);
            else if (_hoveredComponent == (int)SceneGizmoComponent.PositiveY) EditorCamera.Instance.AlignLookWithWorldAxis(Axis.Y, false, _cameraAlignDuration);
            else if (_hoveredComponent == (int)SceneGizmoComponent.NegativeY) EditorCamera.Instance.AlignLookWithWorldAxis(Axis.Y, true, _cameraAlignDuration);
            else if (_hoveredComponent == (int)SceneGizmoComponent.PositiveZ) EditorCamera.Instance.AlignLookWithWorldAxis(Axis.Z, false, _cameraAlignDuration);
            else if (_hoveredComponent == (int)SceneGizmoComponent.NegativeZ) EditorCamera.Instance.AlignLookWithWorldAxis(Axis.Z, true, _cameraAlignDuration);
        }

        private int DetectHoveredComponent()
        {
            int hoveredComponent = -1;
            if (_gizmoCamera == null) return _hoveredComponent;

            Ray pickRay;
            bool canPick = InputDevice.Instance.GetPickRay(_gizmoCamera, out pickRay);
            if (!canPick) return -1;

            Matrix4x4[] transformMatrices = GetComponentTransforms();
            float closestDistance = float.MaxValue;
            float t;

            // Check if the cube component is hovered
            if (pickRay.IntersectsBox(1.0f, 1.0f, 1.0f, transformMatrices[(int)SceneGizmoComponent.Cube], out t))
            {
                closestDistance = t;
                hoveredComponent = (int)SceneGizmoComponent.Cube;
            }

            int numberOfComponents = Enum.GetValues(typeof(SceneGizmoComponent)).Length;
            for (int axisIndex = (int)SceneGizmoComponent.Cube + 1; axisIndex < numberOfComponents; ++axisIndex)
            {
                if (_componentAlphas[axisIndex] != 1.0f) continue;
                if (pickRay.IntersectsCone(1.0f, 1.0f, transformMatrices[axisIndex], out t))
                {
                    if (t < closestDistance)
                    {
                        closestDistance = t;
                        hoveredComponent = axisIndex;
                    }
                }
            }

            return hoveredComponent;
        }

        private void EstablishComponentColors()
        {
            _componentColors[(int)SceneGizmoComponent.Cube] = _cubeColor;
            _componentColors[(int)SceneGizmoComponent.Cube].a = _componentAlphas[(int)SceneGizmoComponent.Cube];
            _componentColors[(int)SceneGizmoComponent.PositiveX] = _xAxisColor;
            _componentColors[(int)SceneGizmoComponent.PositiveX].a = _componentAlphas[(int)SceneGizmoComponent.PositiveX];
            _componentColors[(int)SceneGizmoComponent.PositiveY] = _yAxisColor;
            _componentColors[(int)SceneGizmoComponent.PositiveY].a = _componentAlphas[(int)SceneGizmoComponent.PositiveY];
            _componentColors[(int)SceneGizmoComponent.PositiveZ] = _zAxisColor;
            _componentColors[(int)SceneGizmoComponent.PositiveZ].a = _componentAlphas[(int)SceneGizmoComponent.PositiveZ];
            _componentColors[(int)SceneGizmoComponent.NegativeX] = _negativeAxisColor;
            _componentColors[(int)SceneGizmoComponent.NegativeX].a = _componentAlphas[(int)SceneGizmoComponent.NegativeX];
            _componentColors[(int)SceneGizmoComponent.NegativeY] = _negativeAxisColor;
            _componentColors[(int)SceneGizmoComponent.NegativeY].a = _componentAlphas[(int)SceneGizmoComponent.NegativeY];
            _componentColors[(int)SceneGizmoComponent.NegativeZ] = _negativeAxisColor;
            _componentColors[(int)SceneGizmoComponent.NegativeZ].a = _componentAlphas[(int)SceneGizmoComponent.NegativeZ];

            if (_hoveredComponent >= 0) _componentColors[_hoveredComponent] = _hoveredComponentColor;
        }

        private void OnRenderObject()
        {
            if (Camera.current != _gizmoCamera) return;

            Material material = MaterialPool.Instance.GizmoSolidComponent;
            material.SetVector("_LightDir", _gizmoCameraTransform.forward);
            material.SetInt("_IsLit", 1);
            material.SetInt("_ZTest", 2);
            material.SetFloat("_LightIntensity", 1.23f);

            Mesh coneMesh = MeshPool.Instance.ConeMesh;
            Matrix4x4[] transformMatrices = GetComponentTransforms();

            // Render cube mesh
            material.SetColor("_Color", _componentColors[(int)SceneGizmoComponent.Cube]);
            material.SetInt("_ZWrite", 1);
            material.SetPass(0);
            Graphics.DrawMeshNow(MeshPool.Instance.BoxMesh, transformMatrices[(int)SceneGizmoComponent.Cube]);

            int axesCount = Enum.GetValues(typeof(SceneGizmoComponent)).Length;
            int fadingAxis0 = -1;
            int fadingAxis1 = -1;
            for (int axisIndex = 1; axisIndex < axesCount; ++axisIndex)
            {
                if (_componentAlphaFadeInfo[axisIndex].IsActive)
                {
                    if (fadingAxis0 == -1) fadingAxis0 = axisIndex;
                    if (fadingAxis0 != -1) fadingAxis1 = axisIndex;
                    if (fadingAxis0 != -1 && fadingAxis1 != -1) break;
                }
            }

            for (int axisIndex = 1; axisIndex < axesCount; ++axisIndex)
            {
                if (fadingAxis0 == axisIndex || fadingAxis1 == axisIndex) continue;
                material.SetInt("_ZWrite", GetZWriteForComponent((SceneGizmoComponent)axisIndex));
                material.SetColor("_Color", _componentColors[axisIndex]);
                material.SetPass(0);
                Graphics.DrawMeshNow(coneMesh, transformMatrices[axisIndex]);
            }

            if (fadingAxis0 != -1)
            {
                material.SetInt("_ZWrite", GetZWriteForComponent((SceneGizmoComponent)fadingAxis0));
                material.SetColor("_Color", _componentColors[fadingAxis0]);
                material.SetPass(0);
                Graphics.DrawMeshNow(coneMesh, transformMatrices[fadingAxis0]);
            }

            if (fadingAxis1 != -1)
            {
                material.SetInt("_ZWrite", GetZWriteForComponent((SceneGizmoComponent)fadingAxis1));
                material.SetColor("_Color", _componentColors[fadingAxis1]);
                material.SetPass(0);
                Graphics.DrawMeshNow(coneMesh, transformMatrices[fadingAxis1]);
            }

            // Draw axis cone labels
            material = MaterialPool.Instance.TintedDiffuse;
            material.SetInt("_ZWrite", 0);
            Mesh squareMesh = MeshPool.Instance.XYSquareMesh;

            float axisLabelSquareSize = CalculateAxisLabelSquareSize();
            Vector3 axisLabelScale = new Vector3(axisLabelSquareSize, axisLabelSquareSize, axisLabelSquareSize);
            Quaternion axisLabelRotation = _gizmoCameraTransform.rotation;
            Vector3 cubePos = CalculateCubePosition();
            float gizmoExtent = CalculateGizmoExtent();
            float axisConeRadius = CalculateAxisConeRadius();

            Vector3[] gizmoAxes = new Vector3[] { _gizmoTransform.right, _gizmoTransform.up, _gizmoTransform.forward };
            for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
            {
                Color color = Color.white;
                color.a = _componentAlphas[axisIndex * 2 + 1];

                material.SetColor("_Color", color);
                material.SetTexture("_MainTex", _axisLabelTextures[axisIndex]);
                material.SetPass(0);

                float dotCamLook = Vector3.Dot(gizmoAxes[axisIndex], _gizmoCameraTransform.forward);
                float offsetAlongAxis = (gizmoExtent - axisLabelSquareSize * 0.5f);
                float additionalOffset = Mathf.Lerp(0.0f, axisLabelSquareSize * 0.5f + axisConeRadius * 0.5f, Mathf.Abs(dotCamLook));
                float offsetAlongGizmoUp = 0.0f;
                if (dotCamLook > 0.0f) offsetAlongGizmoUp = Mathf.Lerp(0.0f, axisLabelSquareSize * 0.5f, dotCamLook);

                Vector3 labelPos = cubePos + gizmoAxes[axisIndex] * (offsetAlongAxis + additionalOffset) + _gizmoTransform.up * offsetAlongGizmoUp;
                Graphics.DrawMeshNow(squareMesh, Matrix4x4.TRS(labelPos, axisLabelRotation, axisLabelScale));
            }
        }

        private int GetZWriteForComponent(SceneGizmoComponent comp)
        {
            if (comp == SceneGizmoComponent.Cube) return 1;

            if (_componentAlphas[(int)comp] == 1.0f) return 1;
            return 0;
        }

        private Matrix4x4[] GetComponentTransforms()
        {
            Vector3 cubePosition = CalculateCubePosition();
            float axisConeLength = CalculateAxisConeLength();
            float axisConeRadius = CalculateAxisConeRadius();
            float cubeSideLength = CalculateCubeSideLength();
            Vector3 coneScale = Vector3.Scale(Vector3.one, new Vector3(axisConeRadius, axisConeLength, axisConeRadius));

            Matrix4x4[] transformMatrices = new Matrix4x4[Enum.GetValues(typeof(SceneGizmoComponent)).Length];
            transformMatrices[(int)SceneGizmoComponent.Cube] = Matrix4x4.TRS(cubePosition, _gizmoTransform.rotation, Vector3.one * cubeSideLength);
            transformMatrices[(int)SceneGizmoComponent.PositiveX] = Matrix4x4.TRS(cubePosition + _gizmoTransform.right * (cubeSideLength * 0.5f + axisConeLength),
                                                                                  _gizmoTransform.rotation * Quaternion.Euler(0.0f, 0.0f, 90.0f), coneScale);
            transformMatrices[(int)SceneGizmoComponent.PositiveY] = Matrix4x4.TRS(cubePosition + _gizmoTransform.up * (cubeSideLength * 0.5f + axisConeLength),
                                                                                  _gizmoTransform.rotation * Quaternion.Euler(0.0f, 0.0f, 180.0f), coneScale);
            transformMatrices[(int)SceneGizmoComponent.PositiveZ] = Matrix4x4.TRS(cubePosition + _gizmoTransform.forward * (cubeSideLength * 0.5f + axisConeLength),
                                                                                  _gizmoTransform.rotation * Quaternion.Euler(-90.0f, 0.0f, 0.0f), coneScale);
            transformMatrices[(int)SceneGizmoComponent.NegativeX] = Matrix4x4.TRS(cubePosition - _gizmoTransform.right * (cubeSideLength * 0.5f + axisConeLength),
                                                                                  _gizmoTransform.rotation * Quaternion.Euler(0.0f, 0.0f, -90.0f), coneScale);
            transformMatrices[(int)SceneGizmoComponent.NegativeY] = Matrix4x4.TRS(cubePosition - _gizmoTransform.up * (cubeSideLength * 0.5f + axisConeLength),
                                                                                  _gizmoTransform.rotation * Quaternion.identity, coneScale);
            transformMatrices[(int)SceneGizmoComponent.NegativeZ] = Matrix4x4.TRS(cubePosition - _gizmoTransform.forward * (cubeSideLength * 0.5f + axisConeLength),
                                                                                  _gizmoTransform.rotation * Quaternion.Euler(90.0f, 0.0f, 0.0f), coneScale);

            return transformMatrices;
        }

        private void CreateSceneGizmoCamera()
        {
            GameObject gizmoCameraObj = new GameObject("Scene Gizmo Camera");
            _gizmoCamera = gizmoCameraObj.AddComponent<Camera>();
            _gizmoCameraTransform = _gizmoCamera.transform;
            _gizmoCamera.cullingMask = 0;
            _gizmoCamera.clearFlags = CameraClearFlags.Depth;
            _gizmoCamera.depth = EditorCamera.Instance.Camera.depth + 1.0f;
            _gizmoCamera.renderingPath = RenderingPath.Forward;
            _gizmoCameraTransform.parent = _gizmoTransform.parent;
        }

        private void UpdateGizmoCamera()
        {
            Transform editorCameraTransform = EditorCamera.Instance.transform;
            _gizmoCameraTransform.position = editorCameraTransform.position;
            _gizmoCameraTransform.rotation = editorCameraTransform.rotation;
            UpdateGizmoCameraViewport();
            UpdateGizmoCameraViewVolume();

            if (_gizmoCamera.orthographic != EditorCamera.Instance.Camera.orthographic) _gizmoCamera.orthographic = EditorCamera.Instance.Camera.orthographic;
        }
        public Vector2 offset;
        private void UpdateGizmoCameraViewport()
        {
            Rect editorCamViewRect = EditorCamera.Instance.Camera.pixelRect;
            if (_corner == SceneGizmoCorner.TopRight)
                _gizmoCamera.pixelRect = new Rect(editorCamViewRect.xMax - _screenSize + offset.x, editorCamViewRect.yMax - _screenSize + offset.y, _screenSize, _screenSize);
            else
            if (_corner == SceneGizmoCorner.TopLeft)
                _gizmoCamera.pixelRect = new Rect(editorCamViewRect.xMin + offset.x, editorCamViewRect.yMax - _screenSize, _screenSize + offset.y, _screenSize);
            else
            if (_corner == SceneGizmoCorner.BottomRight)
                _gizmoCamera.pixelRect = new Rect(editorCamViewRect.xMax - _screenSize + offset.x, 10.0f + editorCamViewRect.yMin + offset.y, _screenSize, _screenSize);
            else
                _gizmoCamera.pixelRect = new Rect(editorCamViewRect.xMin + offset.x, 10.0f + editorCamViewRect.yMin, _screenSize + offset.y, _screenSize);
        }

        private void UpdateGizmoCameraViewVolume()
        {
            if (_gizmoCamera.orthographic) _gizmoCamera.orthographicSize = CalculateGizmoExtent() + 0.005f;
            else _gizmoCamera.fieldOfView = 1.1f + 2.0f * Mathf.Rad2Deg * Mathf.Atan2(CalculateGizmoExtent(), CalculateGizmoOffsetFromCamera());
        }

        private float CalculateGizmoOffsetFromCamera()
        {
            return (_gizmoCamera.nearClipPlane + CalculateGizmoExtent() + 0.01f);
        }

        private float CalculateGizmoExtent()
        {
            return (CalculateCubeSideLength() * 0.5f + CalculateAxisConeLength() + CalculateAxisLabelSquareSize() + CalculateAxisLabelOffsetFromCone());
        }

        private Vector3 CalculateCubePosition()
        {
            return _gizmoCameraTransform.position + _gizmoCameraTransform.forward * CalculateGizmoOffsetFromCamera();
        }

        private Quaternion CalculateGizmoRotation()
        {
            return Quaternion.identity;
        }

        private float CalculateCubeSideLength()
        {
            return 0.0075f;
        }

        private float CalculateAxisConeLength()
        {
            return 1.45f * CalculateCubeSideLength();
        }

        private float CalculateAxisConeRadius()
        {
            return CalculateCubeSideLength() * 0.5f;
        }

        private float CalculateAxisLabelSquareSize()
        {
            return CalculateAxisConeRadius() * 1.5f;
        }

        private float CalculateAxisLabelOffsetFromCone()
        {
            return 0.003f;
        }
        #endregion

        #region Coroutines
        private IEnumerator DoComponentAlphaTransitions()
        {
            const float fadeOutThreshold = 0.89f;
            const float fadeDuration = 0.189f;
            int numberOfAxes = Enum.GetValues(typeof(SceneGizmoComponent)).Length;
            Vector3[] gizmoAxes = new Vector3[] { _gizmoTransform.right, -_gizmoTransform.right, _gizmoTransform.up, -_gizmoTransform.up, _gizmoTransform.forward, -_gizmoTransform.forward };

            while (true)
            {
                Vector3 cameraLook = _gizmoCameraTransform.forward;

                for (int axisIndex = 0; axisIndex < numberOfAxes; ++axisIndex)
                {
                    if ((SceneGizmoComponent)axisIndex == SceneGizmoComponent.Cube)
                    {
                        _componentAlphas[axisIndex] = 1.0f;
                        continue;
                    }

                    GizmoCompAlphaFadeInfo fadeInfo = _componentAlphaFadeInfo[axisIndex];
                    float absDot = Mathf.Abs(Vector3.Dot(cameraLook, gizmoAxes[axisIndex - 1]));

                    if (!fadeInfo.IsActive)
                    {
                        if (absDot >= fadeOutThreshold) fadeInfo.ChangeTransition(GizmoCompTransition.FadeOut, _componentAlphas[axisIndex]);
                        else fadeInfo.ChangeTransition(GizmoCompTransition.FadeIn, _componentAlphas[axisIndex]);
                    }

                    if (fadeInfo.IsActive)
                    {
                        _componentAlphas[axisIndex] = Mathf.Lerp(fadeInfo.SrcAlpha, fadeInfo.DestAlpha, fadeInfo.ElapsedTime / fadeDuration);
                        fadeInfo.ElapsedTime += Time.deltaTime;

                        if (fadeInfo.ElapsedTime >= fadeDuration)
                        {
                            _componentAlphas[axisIndex] = fadeInfo.DestAlpha;
                            fadeInfo.Stop();
                        }
                    }
                }

                yield return null;
            }
        }
        #endregion
    }
}
