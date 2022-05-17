using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
namespace RTEditor
{
    public class TranslationGizmo : Gizmo
    {
        public Action<Vector3> OnMove;
        private enum VertexSnapMode
        {
            Vertex = 0,
            Box
        }
        #region Private Variables
        [SerializeField]
        private float _axisLength = 5.0f;
        [SerializeField]
        private float _arrowConeRadius = 0.4f;
        [SerializeField]
        private float _arrowConeLength = 1.19f;
        [SerializeField]
        private float _multiAxisSquareSize = 1.0f;
        [SerializeField]
        private bool _adjustMultiAxisForBetterVisibility = true;
        [SerializeField]
        private float _multiAxisSquareAlpha = 0.2f;
        [SerializeField]
        private bool _areArrowConesLit = true;
        private Dictionary<GameObject, Vector3> _objectOffsetsFromGizmo = new Dictionary<GameObject, Vector3>();
        [SerializeField]
        private Color _specialOpSquareColor = Color.white;
        [SerializeField]
        private Color _specialOpSquareColorWhenSelected = Color.yellow;
        private bool _isSpecialOpSquareSelected;
        [SerializeField]
        private float _screenSizeOfSpecialOpSquare = 25.0f;
        private TranslationGizmoSnapSettings _snapSettings = new TranslationGizmoSnapSettings();
        private Vector3 _accumulatedTranslation;
        private MultiAxisSquare _selectedMultiAxisSquare = MultiAxisSquare.None;
        private bool _isCameraAxesTranslationSquareSelected;
        [SerializeField]
        private int _vertexSnapLayers = ~0;
        [SerializeField]
        private float _moveScale = 0.5f;
        #endregion
        #region Public Static Properties
        public static float MinAxisLength { get { return 0.1f; } }
        public static float MinArrowConeRadius { get { return 0.1f; } }
        public static float MinArrowConeLength { get { return 0.1f; } }
        public static float MinMultiAxisSquareSize { get { return 0.1f; } }
        public static float MinScreenSizeOfCameraAxesTranslationSquare { get { return 2.0f; } }
        public static float MinScreenSizeOfVertexSnappingSquare { get { return 2.0f; } }
        #endregion
        #region Public Properties
        public float MoveScale { get { return _moveScale; } set { _moveScale = Mathf.Clamp(value, 1e-2f, 1.0f); } }
        public float AxisLength { get { return _axisLength; } set { _axisLength = Mathf.Max(MinAxisLength, value); } }
        public float ArrowConeRadius
        {
            get { return _arrowConeRadius; }
            set { _arrowConeRadius = Mathf.Max(MinArrowConeRadius, value); }
        }
        public float ArrowConeLength
        {
            get { return _arrowConeLength; }
            set { _arrowConeLength = Mathf.Max(MinArrowConeLength, value); }
        }
        public float MultiAxisSquareSize
        {
            get { return _multiAxisSquareSize; }
            set { _multiAxisSquareSize = Mathf.Max(MinMultiAxisSquareSize, value); }
        }
        public bool AdjustMultiAxisForBetterVisibility { get { return _adjustMultiAxisForBetterVisibility; } set { _adjustMultiAxisForBetterVisibility = value; } }
        public float MultiAxisSquareAlpha { get { return _multiAxisSquareAlpha; } set { _multiAxisSquareAlpha = Mathf.Clamp(value, 0.0f, 1.0f); } }
        public bool AreArrowConesLit { get { return _areArrowConesLit; } set { _areArrowConesLit = value; } }
        public float ScreenSizeOfSpecialOpSquare { get { return _screenSizeOfSpecialOpSquare; } set { _screenSizeOfSpecialOpSquare = Mathf.Max(value, MinScreenSizeOfVertexSnappingSquare); } }
        public Color SpecialOpSquareColor { get { return _specialOpSquareColor; } set { _specialOpSquareColor = value; } }
        public Color SpecialOpSquareColorWhenSelected { get { return _specialOpSquareColorWhenSelected; } set { _specialOpSquareColorWhenSelected = value; } }
        public TranslationGizmoSnapSettings SnapSettings { get { return _snapSettings; } }
        #endregion
        #region Public Methods
        public bool IsVertexSnapLayerBitSet(int layerNumber)
        {
            return LayerHelper.IsLayerBitSet(_vertexSnapLayers, layerNumber);
        }
        public void SetVertexSnapLayerBit(int layerNumber, bool set)
        {
            if (set) _vertexSnapLayers = LayerHelper.SetLayerBit(_vertexSnapLayers, layerNumber);
            else _vertexSnapLayers = LayerHelper.ClearLayerBit(_vertexSnapLayers, layerNumber);
        }
        public override bool IsReadyForObjectManipulation()
        {
            return _selectedAxis != GizmoAxis.None ||
                   _selectedMultiAxisSquare != MultiAxisSquare.None || _isCameraAxesTranslationSquareSelected ||
                   DetectHoveredComponents(false);
        }
        public override GizmoType GetGizmoType()
        {
            return GizmoType.Translation;
        }

        public void Flush(Vector3 pos, Quaternion ro)
        {
            transform.position = pos;
            transform.rotation = ro;
        }
        #endregion
        #region Protected Methods
        protected override void Start()
        {
            base.Start();
        }
        protected override void Update()
        {
            base.Update();
            //if (InputDevice.Instance.IsPressed(0)) return;
            //DetectHoveredComponents(true);
        }
        protected override void OnInputDeviceButtonDown()
        {
            base.OnInputDeviceButtonDown();
            //if (InputDevice.Instance.UsingMobile) DetectHoveredComponents(true);
            if (Input.GetMouseButton(0))
            {
                if (_selectedAxis != GizmoAxis.None || _selectedMultiAxisSquare != MultiAxisSquare.None || _isCameraAxesTranslationSquareSelected)
                {
                    Plane coordinateSystemPlane;
                    if (_selectedAxis != GizmoAxis.None) coordinateSystemPlane = GetCoordinateSystemPlaneFromSelectedAxis();
                    else if (_selectedMultiAxisSquare != MultiAxisSquare.None) coordinateSystemPlane = GetPlaneFromSelectedMultiAxisSquare();
                    else coordinateSystemPlane = GetCameraAxesTranslationSquarePlane();
                    //Ray pickRay;
                    //bool canPick = InputDevice.Instance.GetPickRay(_camera, out pickRay);
                    float t;
                    Ray pickRay = _camera.ScreenPointToRay(Input.mousePosition);
                    if (/*canPick &&*/ coordinateSystemPlane.Raycast(pickRay, out t)) _lastGizmoPickPoint = pickRay.origin + pickRay.direction * t;
                }
            }
            _accumulatedTranslation = Vector3.zero;
        }
        protected override void OnInputDeviceButtonUp()
        {
            base.OnInputDeviceButtonUp();
            _objectOffsetsFromGizmo.Clear();
            if (_selectedAxis != GizmoAxis.None || _selectedMultiAxisSquare != MultiAxisSquare.None || _isCameraAxesTranslationSquareSelected)
            {
            }
        }
        protected override void OnInputDeviceOver()
        {
            base.OnInputDeviceOver();
            //if (!CanAnyControlledObjectBeManipulated()) return;
            if (!Input.GetMouseButton(0))
            {
                DetectHoveredComponents(true);
                return;
            }
            if (_selectedAxis != GizmoAxis.None)
            {
                Vector3 gizmoMoveAxis;
                if (_selectedAxis == GizmoAxis.X) gizmoMoveAxis = _gizmoTransform.right;
                else if (_selectedAxis == GizmoAxis.Y) gizmoMoveAxis = _gizmoTransform.up;
                else gizmoMoveAxis = _gizmoTransform.forward;
                Plane planeFromSelectedAxis = GetCoordinateSystemPlaneFromSelectedAxis();
                Ray pickRay = _camera.ScreenPointToRay(Input.mousePosition);
                float t;
                if (/*canPick &&*/ planeFromSelectedAxis.Raycast(pickRay, out t))
                {
                    Vector3 intersectionPoint = pickRay.origin + pickRay.direction * t;
                    Vector3 offsetVector = intersectionPoint - _lastGizmoPickPoint;
                    float projectionOnAxis = Vector3.Dot(offsetVector, gizmoMoveAxis);
                    _lastGizmoPickPoint = intersectionPoint;
                    if (/*IsStepSnappingShActive*/ false)
                    {
                        int axisIndex = (int)_selectedAxis;
                        _accumulatedTranslation[axisIndex] += projectionOnAxis;
                        if (Mathf.Abs(_accumulatedTranslation[axisIndex]) >= _snapSettings.StepValueInWorldUnits)
                        {
                            float numberOfFullSteps = (float)((int)(Mathf.Abs(_accumulatedTranslation[axisIndex] / _snapSettings.StepValueInWorldUnits)));
                            float translationAmount = _snapSettings.StepValueInWorldUnits * numberOfFullSteps * Mathf.Sign(_accumulatedTranslation[axisIndex]);
                            Vector3 translationVector = gizmoMoveAxis * translationAmount;
                            _gizmoTransform.position += translationVector;
                            TranslateControlledObjects(translationVector);
                            if (_accumulatedTranslation[axisIndex] > 0.0f) _accumulatedTranslation[axisIndex] -= _snapSettings.StepValueInWorldUnits * numberOfFullSteps;
                            else if (_accumulatedTranslation[axisIndex] < 0.0f) _accumulatedTranslation[axisIndex] += _snapSettings.StepValueInWorldUnits * numberOfFullSteps;
                        }
                    }
                    else
                    {
                        Vector3 translationVector = gizmoMoveAxis * projectionOnAxis;
                        _gizmoTransform.position += translationVector;
                        TranslateControlledObjects(translationVector);
                    }
                }
            }
            else
                            if (_selectedMultiAxisSquare != MultiAxisSquare.None)
            {
                float[] signs = GetMultiAxisExtensionSigns(_adjustMultiAxisForBetterVisibility);
                Vector3 gizmoMoveAxis1, gizmoMoveAxis2;
                int indexOfFirstMoveAxis, indexOfSecondMoveAxis;
                if (_selectedMultiAxisSquare == MultiAxisSquare.XY)
                {
                    gizmoMoveAxis1 = _gizmoTransform.right * signs[0];
                    gizmoMoveAxis2 = _gizmoTransform.up * signs[1];
                    indexOfFirstMoveAxis = 0;
                    indexOfSecondMoveAxis = 1;
                }
                else if (_selectedMultiAxisSquare == MultiAxisSquare.XZ)
                {
                    gizmoMoveAxis1 = _gizmoTransform.right * signs[0];
                    gizmoMoveAxis2 = _gizmoTransform.forward * signs[2];
                    indexOfFirstMoveAxis = 0;
                    indexOfSecondMoveAxis = 2;
                }
                else
                {
                    gizmoMoveAxis1 = _gizmoTransform.up * signs[1];
                    gizmoMoveAxis2 = _gizmoTransform.forward * signs[2];
                    indexOfFirstMoveAxis = 1;
                    indexOfSecondMoveAxis = 2;
                }
                Plane planeFromSelectedMultiAxisSquare = GetPlaneFromSelectedMultiAxisSquare();
                //Ray pickRay;
                //bool canPick = InputDevice.Instance.GetPickRay(_camera, out pickRay);
                Ray pickRay = _camera.ScreenPointToRay(Input.mousePosition);
                float t;
                if (/*canPick && */planeFromSelectedMultiAxisSquare.Raycast(pickRay, out t))
                {
                    Vector3 intersectionPoint = pickRay.origin + pickRay.direction * t;
                    Vector3 offsetVector = intersectionPoint - _lastGizmoPickPoint;
                    float projectionOnFirstAxis = Vector3.Dot(offsetVector, gizmoMoveAxis1);
                    float projectionOnSecondAxis = Vector3.Dot(offsetVector, gizmoMoveAxis2);
                    _lastGizmoPickPoint = intersectionPoint;
                    if (/*IsStepSnappingShActive*/false)
                    {
                        _accumulatedTranslation[indexOfFirstMoveAxis] += projectionOnFirstAxis;
                        _accumulatedTranslation[indexOfSecondMoveAxis] += projectionOnSecondAxis;
                        Vector3 translationVector = Vector3.zero;
                        if (Mathf.Abs(_accumulatedTranslation[indexOfFirstMoveAxis]) >= _snapSettings.StepValueInWorldUnits)
                        {
                            float numberOfFullSteps = (float)((int)(Mathf.Abs(_accumulatedTranslation[indexOfFirstMoveAxis] / _snapSettings.StepValueInWorldUnits)));
                            float translationAmount = _snapSettings.StepValueInWorldUnits * numberOfFullSteps * Mathf.Sign(_accumulatedTranslation[indexOfFirstMoveAxis]);
                            translationVector += gizmoMoveAxis1 * translationAmount;
                            if (_accumulatedTranslation[indexOfFirstMoveAxis] > 0.0f) _accumulatedTranslation[indexOfFirstMoveAxis] -= _snapSettings.StepValueInWorldUnits * numberOfFullSteps;
                            else if (_accumulatedTranslation[indexOfFirstMoveAxis] < 0.0f) _accumulatedTranslation[indexOfFirstMoveAxis] += _snapSettings.StepValueInWorldUnits * numberOfFullSteps;
                        }
                        if (Mathf.Abs(_accumulatedTranslation[indexOfSecondMoveAxis]) >= _snapSettings.StepValueInWorldUnits)
                        {
                            float numberOfFullSteps = (float)((int)(Mathf.Abs(_accumulatedTranslation[indexOfSecondMoveAxis] / _snapSettings.StepValueInWorldUnits)));
                            float translationAmount = _snapSettings.StepValueInWorldUnits * numberOfFullSteps * Mathf.Sign(_accumulatedTranslation[indexOfSecondMoveAxis]);
                            translationVector += gizmoMoveAxis2 * translationAmount;
                            if (_accumulatedTranslation[indexOfSecondMoveAxis] > 0.0f) _accumulatedTranslation[indexOfSecondMoveAxis] -= _snapSettings.StepValueInWorldUnits * numberOfFullSteps;
                            else if (_accumulatedTranslation[indexOfSecondMoveAxis] < 0.0f) _accumulatedTranslation[indexOfSecondMoveAxis] += _snapSettings.StepValueInWorldUnits * numberOfFullSteps;
                        }
                        _gizmoTransform.position += translationVector;
                        TranslateControlledObjects(translationVector);
                    }
                    else
                    {
                        Vector3 translationVector = projectionOnFirstAxis * gizmoMoveAxis1 + projectionOnSecondAxis * gizmoMoveAxis2;
                        _gizmoTransform.position += translationVector;
                        TranslateControlledObjects(translationVector);
                    }
                }
            }
            else
                            if (/*IsTranslateAlongScreenAxesShActive &&*/ _isCameraAxesTranslationSquareSelected)
            {
                Plane squarePlane = GetCameraAxesTranslationSquarePlane();
                //Ray pickRay;
                //bool canPick = InputDevice.Instance.GetPickRay(_camera, out pickRay);
                Ray pickRay = _camera.ScreenPointToRay(Input.mousePosition);
                float t;
                if (/*canPick && */squarePlane.Raycast(pickRay, out t))
                {
                    Vector3 intersectionPoint = pickRay.origin + pickRay.direction * t;
                    Vector3 offsetVector = intersectionPoint - _lastGizmoPickPoint;
                    float projectionOnCameraRightVector = Vector3.Dot(offsetVector, _cameraTransform.right);
                    float projectionOnCameraUpvector = Vector3.Dot(offsetVector, _cameraTransform.up);
                    _lastGizmoPickPoint = intersectionPoint;
                    if (/*IsStepSnappingShActive*/ false)
                    {
                        _accumulatedTranslation[0] += projectionOnCameraRightVector;
                        _accumulatedTranslation[1] += projectionOnCameraUpvector;
                        Vector3 translationVector = Vector3.zero;
                        if (Mathf.Abs(_accumulatedTranslation[0]) >= _snapSettings.StepValueInWorldUnits)
                        {
                            float numberOfFullSteps = (float)((int)(Mathf.Abs(_accumulatedTranslation[0] / _snapSettings.StepValueInWorldUnits)));
                            float translationAmount = _snapSettings.StepValueInWorldUnits * numberOfFullSteps * Mathf.Sign(_accumulatedTranslation[0]);
                            translationVector += _cameraTransform.right * translationAmount;
                            if (_accumulatedTranslation[0] > 0.0f) _accumulatedTranslation[0] -= _snapSettings.StepValueInWorldUnits * numberOfFullSteps;
                            else if (_accumulatedTranslation[0] < 0.0f) _accumulatedTranslation[0] += _snapSettings.StepValueInWorldUnits * numberOfFullSteps;
                        }
                        if (Mathf.Abs(_accumulatedTranslation[1]) >= _snapSettings.StepValueInWorldUnits)
                        {
                            float numberOfFullSteps = (float)((int)(Mathf.Abs(_accumulatedTranslation[1] / _snapSettings.StepValueInWorldUnits)));
                            float translationAmount = _snapSettings.StepValueInWorldUnits * numberOfFullSteps * Mathf.Sign(_accumulatedTranslation[1]);
                            translationVector += _cameraTransform.up * translationAmount;
                            if (_accumulatedTranslation[1] > 0.0f) _accumulatedTranslation[1] -= _snapSettings.StepValueInWorldUnits * numberOfFullSteps;
                            else if (_accumulatedTranslation[1] < 0.0f) _accumulatedTranslation[1] += _snapSettings.StepValueInWorldUnits * numberOfFullSteps;
                        }
                        _gizmoTransform.position += translationVector;
                        TranslateControlledObjects(translationVector);
                    }
                    else
                    {
                        Vector3 translationVector = _cameraTransform.right * projectionOnCameraRightVector + _cameraTransform.up * projectionOnCameraUpvector;
                        _gizmoTransform.position += translationVector;
                        TranslateControlledObjects(translationVector);
                    }
                }
            }
            else
            if (/*IsSurfacePlacementShActive &&*/ ControlledObjects != null && _isSpecialOpSquareSelected)
            {
                if (_objectOffsetsFromGizmo.Count == 0)
                {
                    List<GameObject> parents = GameObjectExtensions.GetParentsFromObjectCollection(ControlledObjects);
                    foreach (var parent in parents) _objectOffsetsFromGizmo.Add(parent, parent.transform.position - _gizmoTransform.position);
                }
                Axis alignmentAxis = Axis.Y;
                MouseCursor.Instance.PushObjectPickMaskFlags(MouseCursorObjectPickFlags.ObjectBox | MouseCursorObjectPickFlags.ObjectSprite);
                MouseCursorRayHit cursorRayHit = MouseCursor.Instance.GetRayHit();
                if (cursorRayHit == null && (!cursorRayHit.WasAnObjectHit || !cursorRayHit.WasACellHit)) return;
                List<GameObject> ignoreObjects = new List<GameObject>();
                foreach (var gameObj in ControlledObjects)
                {
                    List<GameObject> allChildrenAndSelf = gameObj.GetAllChildrenIncludingSelf();
                    ignoreObjects.AddRange(allChildrenAndSelf);
                }
                cursorRayHit.SortedObjectRayHits.RemoveAll(item => ignoreObjects.Contains(item.HitObject));
                if (cursorRayHit.WasAnObjectHit && cursorRayHit.ClosestObjectRayHit.WasTerrainHit)
                {
                    Vector3 newGizmoPos = cursorRayHit.ClosestObjectRayHit.HitPoint;
                    //TerrainCollider terrainCollider = cursorRayHit.ClosestObjectRayHit.HitObject.GetComponent<TerrainCollider>();
                    //if (terrainCollider != null)
                    //{
                    //    List<GameObject> topParents = GameObjectExtensions.GetParentsFromObjectCollection(ControlledObjects);
                    //    if (topParents.Count != 0)
                    //    {
                    //        RaycastHit terrainHit;
                    //        foreach (var parent in topParents)
                    //        {
                    //            Transform parentTransform = parent.transform.root;
                    //            parentTransform.position = newGizmoPos + _objectOffsetsFromGizmo[parent];
                    //            Ray terrainPickRay = new Ray(parentTransform.position, -Vector3.up);
                    //            if (terrainCollider.RaycastReverseIfFail(terrainPickRay, out terrainHit))
                    //            {
                    //                parent.PlaceHierarchyOnPlane(terrainHit.point, terrainHit.normal, /*alignAxis ? (int)alignmentAxis :*/ -1);
                    //                IRTEditorEventListener editorEventListener = parent.GetComponent<IRTEditorEventListener>();
                    //                if (editorEventListener != null) editorEventListener.OnAlteredByTransformGizmo(this);
                    //                _objectsWereTransformedSinceLeftMouseButtonWasPressed = true;
                    //            }
                    //        }
                    //        _gizmoTransform.position = newGizmoPos;
                    //    }
                    //}
                }
                else
                if (cursorRayHit.WasAnObjectHit && cursorRayHit.ClosestObjectRayHit.WasMeshHit)
                {
                    Vector3 newGizmoPos = cursorRayHit.ClosestObjectRayHit.HitPoint;
                    GameObject hitMeshObject = cursorRayHit.ClosestObjectRayHit.HitObject;
                    Vector3 hitNormal = cursorRayHit.ClosestObjectRayHit.HitNormal;
                    if (hitMeshObject != null)
                    {
                        List<GameObject> topParents = GameObjectExtensions.GetParentsFromObjectCollection(ControlledObjects);
                        if (topParents.Count != 0)
                        {
                            foreach (var parent in topParents)
                            {
                                Transform parentTransform = parent.transform.root;
                                parentTransform.position = newGizmoPos + _objectOffsetsFromGizmo[parent];
                                GameObjectRayHit meshHit = null;
                                Ray meshPickRay = new Ray(parentTransform.position + hitNormal, -hitNormal);
                                if (hitMeshObject.RaycastMesh(meshPickRay, out meshHit))
                                {
                                    parent.PlaceHierarchyOnPlane(meshHit.HitPoint, meshHit.HitNormal, /*alignAxis ? (int)alignmentAxis :*/ -1);
                                    IRTEditorEventListener editorEventListener = parent.GetComponent<IRTEditorEventListener>();
                                    if (editorEventListener != null) editorEventListener.OnAlteredByTransformGizmo(this);
                                    _objectsWereTransformedSinceLeftMouseButtonWasPressed = true;
                                }
                            }
                            _gizmoTransform.position = newGizmoPos;
                        }
                    }
                }
                else
                if (cursorRayHit.WasACellHit)
                {
                    Plane xzGridPlane = cursorRayHit.GridCellRayHit.HitCell.ParentGrid.Plane;
                    Vector3 hitPoint = cursorRayHit.GridCellRayHit.HitPoint;
                    Vector3 newGizmoPos = hitPoint;
                    List<GameObject> topParents = GameObjectExtensions.GetParentsFromObjectCollection(ControlledObjects);
                    if (topParents.Count != 0)
                    {
                        foreach (var parent in topParents)
                        {
                            Transform parentTransform = parent.transform.root;
                            parentTransform.position = newGizmoPos + _objectOffsetsFromGizmo[parent];
                            float distFromPlane = xzGridPlane.GetDistanceToPoint(parentTransform.position);
                            Vector3 projectedPos = parentTransform.position - distFromPlane * xzGridPlane.normal;
                            parent.PlaceHierarchyOnPlane(projectedPos, xzGridPlane.normal, /*alignAxis ? (int)alignmentAxis :*/ -1);
                            IRTEditorEventListener editorEventListener = parent.GetComponent<IRTEditorEventListener>();
                            if (editorEventListener != null) editorEventListener.OnAlteredByTransformGizmo(this);
                            _objectsWereTransformedSinceLeftMouseButtonWasPressed = true;
                        }
                        _gizmoTransform.position = newGizmoPos;
                    }
                }
            }

        }
        protected override void OnRenderObject()
        {
            base.OnRenderObject();
            float gizmoScale = CalculateGizmoScale();
            Matrix4x4[] arrowConeWorldTransforms = GetArrowConesWorldTransforms();
            DrawArrowCones(arrowConeWorldTransforms);
            if (/*!IsTranslateAlongScreenAxesShActive &&*/ true /*&& !isPlacingObjectsOnSurface*/)
            {
                Matrix4x4[] multiAxisWorldTransforms = GetMultiAxisSquaresWorldTransforms();
                DrawMultiAxisSquares(multiAxisWorldTransforms);
            }
            int[] axisIndices = GetSortedGizmoAxesIndices();
            Vector3[] gizmoLocalAxes = GetGizmoLocalAxes();
            Vector3 startPoint = _gizmoTransform.position;
            foreach (int axisIndex in axisIndices)
            {
                if (!_axesVisibilityMask[axisIndex]) continue;
                Color axisColor = _selectedAxis == (GizmoAxis)axisIndex ? _selectedAxisColor : _axesColors[axisIndex];
                Vector3 endPoint = startPoint + gizmoLocalAxes[axisIndex] * _axisLength * gizmoScale;
                UpdateShaderStencilRefValuesForGizmoAxisLineDraw(axisIndex, startPoint, endPoint, gizmoScale);
                GLPrimitives.Draw3DLine(startPoint, endPoint, axisColor, MaterialPool.Instance.GizmoLine);
            }
            MaterialPool.Instance.GizmoLine.SetInt("_StencilRefValue", _doNotUseStencil);
            if (/*!IsTranslateAlongScreenAxesShActive &&*/ true /*&& !isPlacingObjectsOnSurface*/)
            {
                Vector3[] squareLinesPoints;
                Color[] squareLinesColors;
                GetMultiAxisSquaresLinePointsAndColors(gizmoScale, out squareLinesPoints, out squareLinesColors);
                GLPrimitives.Draw3DLines(squareLinesPoints, squareLinesColors, false, MaterialPool.Instance.GizmoLine, false, Color.black);
            }
            if (/*IsTranslateAlongScreenAxesShActive*/ false)
            {
                Color squareLineColor = _isCameraAxesTranslationSquareSelected ? SpecialOpSquareColorWhenSelected : SpecialOpSquareColor;
                GLPrimitives.Draw2DRectangleBorderLines(GetSpecialOpSquareScreenPoints(), squareLineColor, MaterialPool.Instance.GizmoLine, _camera);
            }
            if (/*isPlacingObjectsOnSurface*/false)
            {
                Color squareLineColor = _isSpecialOpSquareSelected ? _specialOpSquareColorWhenSelected : _specialOpSquareColor;
                GLPrimitives.Draw2DRectangleBorderLines(GetSpecialOpSquareScreenPoints(), squareLineColor, MaterialPool.Instance.GizmoLine, _camera);
            }
        }
        protected override bool DetectHoveredComponents(bool updateCompStates)
        {
            if (updateCompStates)
            {
                _selectedAxis = GizmoAxis.None;
                _selectedMultiAxisSquare = MultiAxisSquare.None;
                _isCameraAxesTranslationSquareSelected = false;
                _isSpecialOpSquareSelected = IsMouseCursorInsideSpecialOpSquare();
                if (_isSpecialOpSquareSelected /*&& isPlacingObjectsOnSurface*/) return false;
                if (_camera == null) return false;
                //Ray pickRay;
                //bool canPick = InputDevice.Instance.GetPickRay(_camera, out pickRay);
                Ray pickRay = _camera.ScreenPointToRay(Input.mousePosition);
                float minimumDistanceFromCamera = float.MaxValue;
                float gizmoScale = CalculateGizmoScale();
                float cylinderRadius = 0.2f * gizmoScale; Vector3 cameraPosition = _cameraTransform.position;
                Vector3 gizmoPosition = _gizmoTransform.position;
                if (true)
                {
                    Matrix4x4[] arrowConeWorldTransforms = GetArrowConesWorldTransforms();
                    float t;
                    Vector3[] gizmoLocalAxes = GetGizmoLocalAxes();
                    Vector3 firstCylinderPoint = gizmoPosition;
                    for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
                    {
                        if (!_axesVisibilityMask[axisIndex]) continue;
                        bool axisWasPicked = false;
                        Vector3 secondCylinderPoint = gizmoPosition + gizmoLocalAxes[axisIndex] * _axisLength * gizmoScale;
                        if (pickRay.IntersectsCylinder(firstCylinderPoint, secondCylinderPoint, cylinderRadius, out t))
                        {
                            Vector3 intersectionPoint = pickRay.origin + pickRay.direction * t;
                            float distanceFromCamera = (intersectionPoint - cameraPosition).magnitude;
                            if (distanceFromCamera < minimumDistanceFromCamera)
                            {
                                minimumDistanceFromCamera = distanceFromCamera;
                                _selectedAxis = (GizmoAxis)axisIndex;
                                axisWasPicked = true;
                            }
                        }
                        if (!axisWasPicked && pickRay.IntersectsCone(1.0f, 1.0f, arrowConeWorldTransforms[axisIndex], out t))
                        {
                            Vector3 intersectionPoint = pickRay.origin + pickRay.direction * t;
                            float distanceFromCamera = (intersectionPoint - cameraPosition).magnitude;
                            if (distanceFromCamera < minimumDistanceFromCamera)
                            {
                                minimumDistanceFromCamera = distanceFromCamera;
                                _selectedAxis = (GizmoAxis)axisIndex;
                            }
                        }
                    }
                    if (/*!IsTranslateAlongScreenAxesShActive*/ true)
                    {
                        Vector3[] squarePlaneNormals = new Vector3[] { _gizmoTransform.forward, _gizmoTransform.up, _gizmoTransform.right };
                        float[] signs = GetMultiAxisExtensionSigns(_adjustMultiAxisForBetterVisibility);
                        float gizmoScaleSign = Mathf.Sign(gizmoScale);
                        Vector3[] axesUsedForProjection = new Vector3[]
{
                            _gizmoTransform.right * signs[0], _gizmoTransform.up * signs[1],                                        _gizmoTransform.right * signs[0], _gizmoTransform.forward * signs[2],                                   _gizmoTransform.up * signs[1], _gizmoTransform.forward * signs[2]                                   };
                        for (int multiAxisIndex = 0; multiAxisIndex < 3; ++multiAxisIndex)
                        {
                            if (!IsMultiAxisSquareVisible(multiAxisIndex)) continue;
                            Plane squarePlane = new Plane(squarePlaneNormals[multiAxisIndex], _gizmoTransform.position);
                            if (squarePlane.Raycast(pickRay, out t))
                            {
                                Vector3 intersectionPoint = pickRay.origin + pickRay.direction * t;
                                Vector3 fromGizmoOriginToIntersectPoint = intersectionPoint - _gizmoTransform.position;
                                float projectionOnFirstAxis = Vector3.Dot(fromGizmoOriginToIntersectPoint, axesUsedForProjection[multiAxisIndex * 2]) * gizmoScaleSign;
                                float projectionOnSecondAxis = Vector3.Dot(fromGizmoOriginToIntersectPoint, axesUsedForProjection[multiAxisIndex * 2 + 1]) * gizmoScaleSign;
                                if (projectionOnFirstAxis >= 0.0f && projectionOnFirstAxis <= (_multiAxisSquareSize * Mathf.Abs(gizmoScale)) &&
projectionOnSecondAxis >= 0.0f && projectionOnSecondAxis <= (_multiAxisSquareSize * Mathf.Abs(gizmoScale)))
                                {
                                    float distanceFromCamera = (intersectionPoint - cameraPosition).magnitude;
                                    if (distanceFromCamera < minimumDistanceFromCamera)
                                    {
                                        minimumDistanceFromCamera = distanceFromCamera;
                                        _selectedMultiAxisSquare = (MultiAxisSquare)multiAxisIndex;
                                        _selectedAxis = GizmoAxis.None;
                                    }
                                }
                            }
                        }
                    }
                    if (/*IsTranslateAlongScreenAxesShActive && */IsMouseCursorInsideSpecialOpSquare())
                    {
                        _isCameraAxesTranslationSquareSelected = true;
                        _selectedAxis = GizmoAxis.None;
                        _selectedMultiAxisSquare = MultiAxisSquare.None;
                    }
                }
                return _selectedAxis != GizmoAxis.None || _selectedMultiAxisSquare != MultiAxisSquare.None || _isCameraAxesTranslationSquareSelected;
            }
            else
            {
                if (_camera == null) return false;
                Ray pickRay = _camera.ScreenPointToRay(Input.mousePosition);
                float gizmoScale = CalculateGizmoScale();
                float cylinderRadius = 0.2f * gizmoScale; Vector3 gizmoPosition = _gizmoTransform.position;
                if (true)
                {
                    Matrix4x4[] arrowConeWorldTransforms = GetArrowConesWorldTransforms();
                    float t;
                    Vector3[] gizmoLocalAxes = GetGizmoLocalAxes();
                    Vector3 firstCylinderPoint = gizmoPosition;
                    for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
                    {
                        if (!_axesVisibilityMask[axisIndex]) continue;
                        Vector3 secondCylinderPoint = gizmoPosition + gizmoLocalAxes[axisIndex] * _axisLength * gizmoScale;
                        if (pickRay.IntersectsCylinder(firstCylinderPoint, secondCylinderPoint, cylinderRadius, out t)) return true;
                        if (pickRay.IntersectsCone(1.0f, 1.0f, arrowConeWorldTransforms[axisIndex], out t)) return true;
                    }
                    if (/*!IsTranslateAlongScreenAxesShActive*/ true)
                    {
                        Vector3[] squarePlaneNormals = new Vector3[] { _gizmoTransform.forward, _gizmoTransform.up, _gizmoTransform.right };
                        float[] signs = GetMultiAxisExtensionSigns(_adjustMultiAxisForBetterVisibility);
                        float gizmoScaleSign = Mathf.Sign(gizmoScale);
                        Vector3[] axesUsedForProjection = new Vector3[]
{
                            _gizmoTransform.right * signs[0], _gizmoTransform.up * signs[1],                                        _gizmoTransform.right * signs[0], _gizmoTransform.forward * signs[2],                                   _gizmoTransform.up * signs[1], _gizmoTransform.forward * signs[2]                                   };
                        for (int multiAxisIndex = 0; multiAxisIndex < 3; ++multiAxisIndex)
                        {
                            if (!IsMultiAxisSquareVisible(multiAxisIndex)) continue;
                            Plane squarePlane = new Plane(squarePlaneNormals[multiAxisIndex], _gizmoTransform.position);
                            if (squarePlane.Raycast(pickRay, out t))
                            {
                                Vector3 intersectionPoint = pickRay.origin + pickRay.direction * t;
                                Vector3 fromGizmoOriginToIntersectPoint = intersectionPoint - _gizmoTransform.position;
                                float projectionOnFirstAxis = Vector3.Dot(fromGizmoOriginToIntersectPoint, axesUsedForProjection[multiAxisIndex * 2]) * gizmoScaleSign;
                                float projectionOnSecondAxis = Vector3.Dot(fromGizmoOriginToIntersectPoint, axesUsedForProjection[multiAxisIndex * 2 + 1]) * gizmoScaleSign;
                                if (projectionOnFirstAxis >= 0.0f && projectionOnFirstAxis <= (_multiAxisSquareSize * Mathf.Abs(gizmoScale)) &&
projectionOnSecondAxis >= 0.0f && projectionOnSecondAxis <= (_multiAxisSquareSize * Mathf.Abs(gizmoScale))) return true;
                            }
                        }
                    }
                    if (/*IsTranslateAlongScreenAxesShActive && */IsMouseCursorInsideSpecialOpSquare()) return true;
                }
                return false;
            }
        }
        #endregion
        #region Private Methods   
        private bool IsMultiAxisSquareVisible(int multiAxisIndex)
        {
            MultiAxisSquare multiAxisSquare = (MultiAxisSquare)multiAxisIndex;
            if (multiAxisSquare == MultiAxisSquare.XY)
            {
                if (!_axesVisibilityMask[0] || !_axesVisibilityMask[1]) return false;
            }
            else
            if (multiAxisSquare == MultiAxisSquare.XZ)
            {
                if (!_axesVisibilityMask[0] || !_axesVisibilityMask[2]) return false;
            }
            else
            {
                if (!_axesVisibilityMask[1] || !_axesVisibilityMask[2]) return false;
            }
            return true;
        }
        private void GetMultiAxisSquaresLinePointsAndColors(float gizmoScale, out Vector3[] squareLinesPoints, out Color[] squareLinesColors)
        {
            float multiAxisLineLength = (_multiAxisSquareSize + 0.001f) * gizmoScale;
            squareLinesPoints = new Vector3[24];
            squareLinesColors = new Color[12];
            Vector3[] axesUsedForMultiAxisDraw = GetWorldAxesUsedToDrawMultiAxisSquareLines();
            for (int multiAxisIndex = 0; multiAxisIndex < 3; ++multiAxisIndex)
            {
                Color lineColor = GetMultiAxisSquareLineColor((MultiAxisSquare)multiAxisIndex, _selectedMultiAxisSquare == (MultiAxisSquare)multiAxisIndex);
                if (!IsMultiAxisSquareVisible(multiAxisIndex)) lineColor.a = 0.0f;
                int indexOfFirstColor = multiAxisIndex * 4;
                squareLinesColors[indexOfFirstColor] = lineColor;
                squareLinesColors[indexOfFirstColor + 1] = lineColor;
                squareLinesColors[indexOfFirstColor + 2] = lineColor;
                squareLinesColors[indexOfFirstColor + 3] = lineColor;
                int indexOfFirstDrawAxis = multiAxisIndex * 2;
                Vector3 firstPoint = _gizmoTransform.position;
                Vector3 secondPoint = firstPoint + axesUsedForMultiAxisDraw[indexOfFirstDrawAxis + 1] * multiAxisLineLength;
                Vector3 thirdPoint = secondPoint + axesUsedForMultiAxisDraw[indexOfFirstDrawAxis] * multiAxisLineLength;
                Vector3 fourthPoint = firstPoint + axesUsedForMultiAxisDraw[indexOfFirstDrawAxis] * multiAxisLineLength;
                int indexOfFirstPoint = multiAxisIndex * 8;
                squareLinesPoints[indexOfFirstPoint] = firstPoint;
                squareLinesPoints[indexOfFirstPoint + 1] = secondPoint;
                squareLinesPoints[indexOfFirstPoint + 2] = secondPoint;
                squareLinesPoints[indexOfFirstPoint + 3] = thirdPoint;
                squareLinesPoints[indexOfFirstPoint + 4] = thirdPoint;
                squareLinesPoints[indexOfFirstPoint + 5] = fourthPoint;
                squareLinesPoints[indexOfFirstPoint + 6] = fourthPoint;
                squareLinesPoints[indexOfFirstPoint + 7] = firstPoint;
            }
        }
        private Vector2[] GetSpecialOpSquareScreenPoints()
        {
            Vector2 screenSpaceSquareCenter = _camera.WorldToScreenPoint(_gizmoTransform.position);
            float halfSquareSize = ScreenSizeOfSpecialOpSquare * 0.5f;
            return new Vector2[]
{
                screenSpaceSquareCenter - (Vector2.right - Vector2.up) * halfSquareSize,                        screenSpaceSquareCenter + (Vector2.right + Vector2.up) * halfSquareSize,                        screenSpaceSquareCenter + (Vector2.right - Vector2.up) * halfSquareSize,                        screenSpaceSquareCenter - (Vector2.right + Vector2.up) * halfSquareSize                     };
        }
        private bool IsMouseCursorInsideSpecialOpSquare()
        {
            Vector2 screenSpaceSquareCenter = _camera.WorldToScreenPoint(_gizmoTransform.position);
            float halfSquareSize = ScreenSizeOfSpecialOpSquare * 0.5f;
            Vector2 inputDevPos = Input.mousePosition;
            //if (!InputDevice.Instance.GetPosition(out inputDevPos)) return false;
            Vector2 fromSquareCenterToCursorPosition = inputDevPos - screenSpaceSquareCenter;
            return Mathf.Abs(fromSquareCenterToCursorPosition.x) <= halfSquareSize && Mathf.Abs(fromSquareCenterToCursorPosition.y) <= halfSquareSize;
        }
        private void DrawMultiAxisSquares(Matrix4x4[] worldTransformMatrices)
        {
            Material material = MaterialPool.Instance.GizmoSolidComponent;
            material.SetInt("_ZTest", 0);
            material.SetInt("_ZWrite", 1);
            material.SetVector("_LightDir", _cameraTransform.forward);
            material.SetInt("_IsLit", 0);
            int cullMode = material.GetInt("_CullMode");
            material.SetInt("_CullMode", 0);
            Mesh squareMesh = MeshPool.Instance.XYSquareMesh;
            for (int multiAxisIndex = 0; multiAxisIndex < 3; ++multiAxisIndex)
            {
                if (!IsMultiAxisSquareVisible(multiAxisIndex)) continue;
                Color multiAxisColor = GetMultiAxisSquareColor((MultiAxisSquare)multiAxisIndex, _selectedMultiAxisSquare == (MultiAxisSquare)multiAxisIndex);
                material.SetColor("_Color", multiAxisColor);
                material.SetPass(0);
                Graphics.DrawMeshNow(squareMesh, worldTransformMatrices[multiAxisIndex]);
            }
            material.SetInt("_CullMode", cullMode);
        }
        private Matrix4x4[] GetMultiAxisSquaresWorldTransforms()
        {
            Matrix4x4[] worldTransforms = new Matrix4x4[3];
            Vector3[] localPositions = GetMultiAxisSquaresGizmoLocalPositions();
            Quaternion[] localRotations = GetMultiAxisSquaresGizmoLocalRotations();
            float gizmoScale = CalculateGizmoScale();
            for (int multiAxisIndex = 0; multiAxisIndex < 3; ++multiAxisIndex)
            {
                Vector3 worldPosition = _gizmoTransform.position + _gizmoTransform.rotation * localPositions[multiAxisIndex] * gizmoScale;
                Quaternion worldRotation = _gizmoTransform.rotation * localRotations[multiAxisIndex];
                worldTransforms[multiAxisIndex] = new Matrix4x4();
                worldTransforms[multiAxisIndex].SetTRS(worldPosition, worldRotation, Vector3.Scale(_gizmoTransform.lossyScale, new Vector3(_multiAxisSquareSize, _multiAxisSquareSize, 1.0f)));
            }
            return worldTransforms;
        }
        private Quaternion[] GetMultiAxisSquaresGizmoLocalRotations()
        {
            return new Quaternion[]
            {
                Quaternion.identity,
                Quaternion.Euler(90.0f, 0.0f, 0.0f),
                Quaternion.Euler(0.0f, 90.0f, 0.0f)
            };
        }
        private Vector3[] GetMultiAxisSquaresGizmoLocalPositions()
        {
            float halfSize = _multiAxisSquareSize * 0.5f;
            float[] signs = GetMultiAxisExtensionSigns(_adjustMultiAxisForBetterVisibility);
            return new Vector3[]
{
                (Vector3.right * signs[0] + Vector3.up * signs[1]) * halfSize,
                (Vector3.right * signs[0] + Vector3.forward * signs[2]) * halfSize,
                (Vector3.up * signs[1] + Vector3.forward * signs[2]) * halfSize
};
        }
        private Vector3[] GetWorldAxesUsedToDrawMultiAxisSquareLines()
        {
            float[] signs = GetMultiAxisExtensionSigns(_adjustMultiAxisForBetterVisibility);
            return new Vector3[]
            {
                _gizmoTransform.right * signs[0], _gizmoTransform.up * signs[1],
                _gizmoTransform.right * signs[0], _gizmoTransform.forward * signs[2],
                _gizmoTransform.up * signs[1], _gizmoTransform.forward * signs[2]
            };
        }
        private Plane GetPlaneFromSelectedMultiAxisSquare()
        {
            switch (_selectedMultiAxisSquare)
            {
                case MultiAxisSquare.XY:
                    return new Plane(_gizmoTransform.forward, _gizmoTransform.position);
                case MultiAxisSquare.XZ:
                    return new Plane(_gizmoTransform.up, _gizmoTransform.position);
                case MultiAxisSquare.YZ:
                    return new Plane(_gizmoTransform.right, _gizmoTransform.position);
                default:
                    return new Plane();
            }
        }
        private Plane GetCameraAxesTranslationSquarePlane()
        {
            return new Plane(_cameraTransform.forward, _gizmoTransform.position);
        }

        private void TranslateControlledObjects(Vector3 translationVector)
        {
            OnMove?.Invoke(translationVector);
            return;
            if (ControlledObjects != null)
            {
                List<GameObject> topParents = GetParentsFromControlledObjects(true);
                bool canUseAxisMask = (!_isCameraAxesTranslationSquareSelected /*&& !IsSurfacePlacementShActive*/);
                if (topParents.Count != 0)
                {
                    foreach (GameObject topParent in topParents)
                    {
                        if (topParent != null)
                        {
                            Vector3 moveVector = translationVector;
                            if (canUseAxisMask && _objAxisMask.ContainsKey(topParent))
                            {
                                bool[] mask = _objAxisMask[topParent];
                                for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
                                {
                                    if (!mask[axisIndex]) moveVector[axisIndex] = 0.0f;
                                }
                            }
                            topParent.transform.position += moveVector;
                            //IRTEditorEventListener editorEventListener = topParent.GetComponent<IRTEditorEventListener>();
                            //if (editorEventListener != null) editorEventListener.OnAlteredByTransformGizmo(this);
                            _objectsWereTransformedSinceLeftMouseButtonWasPressed = true;
                        }
                    }
                }
            }
        }
        private void DrawArrowCones(Matrix4x4[] worldTransformMatrices)
        {
            Material material = MaterialPool.Instance.GizmoSolidComponent;
            material.SetInt("_ZTest", 0);
            material.SetInt("_ZWrite", 1);
            material.SetVector("_LightDir", _cameraTransform.forward);
            material.SetInt("_IsLit", _areArrowConesLit ? 1 : 0);
            material.SetFloat("_LightIntensity", 1.5f);
            Mesh coneMesh = MeshPool.Instance.ConeMesh;
            for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
            {
                if (!_axesVisibilityMask[axisIndex]) continue;
                Color axisColor = axisIndex == (int)_selectedAxis ? _selectedAxisColor : _axesColors[axisIndex];
                material.SetInt("_StencilRefValue", _axesStencilRefValues[axisIndex]);
                material.SetColor("_Color", axisColor);
                material.SetPass(0);
                Graphics.DrawMeshNow(coneMesh, worldTransformMatrices[axisIndex]);
            }
        }
        private Matrix4x4[] GetArrowConesWorldTransforms()
        {
            Matrix4x4[] worldTransforms = new Matrix4x4[3];
            Vector3[] localPositions = GetArrowConesGizmoLocalPositions();
            Quaternion[] localRotations = GetArrowConesGizmoLocalRotations();
            float gizmoScale = CalculateGizmoScale();
            for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
            {
                Vector3 worldPosition = _gizmoTransform.position + _gizmoTransform.rotation * localPositions[axisIndex] * gizmoScale;
                Quaternion worldRotation = _gizmoTransform.rotation * localRotations[axisIndex];
                worldTransforms[axisIndex] = new Matrix4x4();
                worldTransforms[axisIndex].SetTRS(worldPosition, worldRotation, Vector3.Scale(Vector3.one * gizmoScale, new Vector3(_arrowConeRadius, _arrowConeLength, _arrowConeRadius)));
            }
            return worldTransforms;
        }
        private Vector3[] GetArrowConesGizmoLocalPositions()
        {
            return new Vector3[]
            {
                Vector3.right * _axisLength,
                Vector3.up * _axisLength,
                Vector3.forward * _axisLength
            };
        }
        private Quaternion[] GetArrowConesGizmoLocalRotations()
        {
            return new Quaternion[]
            {
                Quaternion.Euler(0.0f, 0.0f, -90.0f),
                Quaternion.identity,
                Quaternion.Euler(90.0f, 0.0f, 0.0f)
            };
        }
        private Color GetMultiAxisSquareColor(MultiAxisSquare multiAxisSquare, bool isSelected)
        {
            if (multiAxisSquare == MultiAxisSquare.XY)
            {
                if (isSelected) return new Color(_selectedAxisColor.r, _selectedAxisColor.g, _selectedAxisColor.b, _multiAxisSquareAlpha);
                else return new Color(_axesColors[2].r, _axesColors[2].g, _axesColors[2].b, _multiAxisSquareAlpha);
            }
            else if (multiAxisSquare == MultiAxisSquare.XZ)
            {
                if (isSelected) return new Color(_selectedAxisColor.r, _selectedAxisColor.g, _selectedAxisColor.b, _multiAxisSquareAlpha);
                else return new Color(_axesColors[1].r, _axesColors[1].g, _axesColors[1].b, _multiAxisSquareAlpha);
            }
            else
            {
                if (isSelected) return new Color(_selectedAxisColor.r, _selectedAxisColor.g, _selectedAxisColor.b, _multiAxisSquareAlpha);
                else return new Color(_axesColors[0].r, _axesColors[0].g, _axesColors[0].b, _multiAxisSquareAlpha);
            }
        }
        private Color GetMultiAxisSquareLineColor(MultiAxisSquare multiAxisSquare, bool isSelected)
        {
            if (multiAxisSquare == MultiAxisSquare.XY)
            {
                if (isSelected) return new Color(_selectedAxisColor.r, _selectedAxisColor.g, _selectedAxisColor.b, _selectedAxisColor.a);
                else return new Color(_axesColors[2].r, _axesColors[2].g, _axesColors[2].b, _axesColors[2].a);
            }
            else if (multiAxisSquare == MultiAxisSquare.XZ)
            {
                if (isSelected) return new Color(_selectedAxisColor.r, _selectedAxisColor.g, _selectedAxisColor.b, _selectedAxisColor.a);
                else return new Color(_axesColors[1].r, _axesColors[1].g, _axesColors[1].b, _axesColors[1].a);
            }
            else
            {
                if (isSelected) return new Color(_selectedAxisColor.r, _selectedAxisColor.g, _selectedAxisColor.b, _selectedAxisColor.a);
                else return new Color(_axesColors[0].r, _axesColors[0].g, _axesColors[0].b, _axesColors[0].a);
            }
        }
        #endregion
    }
}
