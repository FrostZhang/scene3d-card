using UnityEngine;
using System.Collections.Generic;
using System;

namespace RTEditor
{
    public class VolumeScaleGizmo : Gizmo
    {
        private struct DragHandle
        {
            public Rect ScreenRect;
            public BoxFace VolumeFace;
            public GizmoAxis Axis;
            public bool IsVisible;
        }

        private struct DragStartData
        {
            public BoxFace DragHandleFace;
            public Plane DragHandlePlane;
            public OrientedBox TargetOOBB;
            public Vector3 TargetObjectScale;
            public Vector3 ScalePivot;
            public Plane PivotPlane;
            public Vector3 FromPivotToObjectPos;
            public bool ScaleFromCenter;
            public int ScaleAxisIndex;
            public Plane DragPlane;
        }

        [SerializeField]
        private Color _lineColor = new Color(1.0f, 1.0f, 1.0f, 0.211f);
        [SerializeField]
        private int _dragHandleSizeInPixels = 8;

        [SerializeField]
        private float _snapStepInWorldUnits = 1.0f;

        // Must have one to one maping to BoxFace enum
        private DragHandle[] _dragHandles = new DragHandle[]
        {
            new DragHandle() { Axis = GizmoAxis.Z, VolumeFace = BoxFace.Front },
            new DragHandle() { Axis = GizmoAxis.Z, VolumeFace = BoxFace.Back },
            new DragHandle() { Axis = GizmoAxis.Y, VolumeFace = BoxFace.Top },
            new DragHandle() { Axis = GizmoAxis.Y, VolumeFace = BoxFace.Bottom },
            new DragHandle() { Axis = GizmoAxis.X, VolumeFace = BoxFace.Left },
            new DragHandle() { Axis = GizmoAxis.X, VolumeFace = BoxFace.Right },
        };
        private int _hoveredDragHandle = -1;    // When not -1, maps to BoxFace enum

        private GameObject _targetObject;
        private OrientedBox _targetOOBB;

        private DragStartData _dragStartData = new DragStartData();

        public Color LineColor { get { return _lineColor; } set { _lineColor = value; } }
        public int DragHandleSizeInPixels { get { return _dragHandleSizeInPixels; } set { _dragHandleSizeInPixels = Mathf.Clamp(value, 2, 50); } }
        public float SnapStepInWorldUnits { get { return _snapStepInWorldUnits; } set { _snapStepInWorldUnits = Mathf.Max(value, 1e-1f); } }
        public Action<Vector3, Vector3> OnMove;
        public override GizmoType GetGizmoType()
        {
            return GizmoType.VolumeScale;
        }

        public override bool IsReadyForObjectManipulation()
        {
            return DetectHoveredComponents(false);
        }

        public void RefreshTargets()
        {
            _targetObject = GetTargetObject();
            UpdateTargetOOBB();
        }

        protected override bool DetectHoveredComponents(bool updateCompStates)
        {
            if (updateCompStates && !_isDragging)
            {
                _hoveredDragHandle = GetIndexOfHoveredDragHandle();

                int axisIndex = _hoveredDragHandle / 2;
                if (axisIndex == 0) _selectedAxis = GizmoAxis.X;
                else if (axisIndex == 1) _selectedAxis = GizmoAxis.Y;
                else if (axisIndex == 2) _selectedAxis = GizmoAxis.Z;
                else _selectedAxis = GizmoAxis.None;

                return _hoveredDragHandle != -1;
            }
            else
            {
                return GetIndexOfHoveredDragHandle() != -1;
            }
        }

        protected override void Update()
        {
            base.Update();

            _targetObject = GetTargetObject();
            UpdateTargetOOBB();

            UpdateDragHandles();
            DetectHoveredComponents(true);
        }

        protected override void OnInputDeviceButtonDown()
        {
            base.OnInputDeviceButtonDown();

            if (_hoveredDragHandle != -1 && _targetOOBB != null && _targetObject != null)
            {
                _dragStartData.DragHandleFace = (BoxFace)_hoveredDragHandle;
                _dragStartData.TargetOOBB = new OrientedBox(_targetOOBB);
                _dragStartData.TargetObjectScale = _targetObject.transform.lossyScale;
                //_dragStartData.ScaleFromCenter = _enableScaleFromCenterShortcut.IsActive();
                _dragStartData.DragHandlePlane = _dragStartData.TargetOOBB.GetBoxFacePlane(_dragStartData.DragHandleFace);

                _dragStartData.ScaleAxisIndex = -1;
                if (_dragStartData.DragHandleFace == BoxFace.Left || _dragStartData.DragHandleFace == BoxFace.Right) _dragStartData.ScaleAxisIndex = 0;
                else if (_dragStartData.DragHandleFace == BoxFace.Bottom || _dragStartData.DragHandleFace == BoxFace.Top) _dragStartData.ScaleAxisIndex = 1;
                else _dragStartData.ScaleAxisIndex = 2;

                if (_dragStartData.ScaleFromCenter) _dragStartData.ScalePivot = _dragStartData.TargetOOBB.Center;
                else _dragStartData.ScalePivot = _dragStartData.TargetOOBB.GetBoxFaceCenter(BoxFaces.GetOpposite(_dragStartData.DragHandleFace));
                _dragStartData.PivotPlane = new Plane(_dragStartData.DragHandlePlane.normal, _dragStartData.ScalePivot);

                _dragStartData.DragPlane = CalculateDragPlane();
                _dragStartData.FromPivotToObjectPos = _targetObject.transform.position - _dragStartData.ScalePivot;
            }
        }

        protected override void OnInputDeviceButtonUp()
        {
            base.OnInputDeviceButtonUp();
        }

        protected override void OnInputDeviceOver()
        {
            base.OnInputDeviceOver();
            if (_isDragging && _targetObject != null && _targetOOBB != null)
            {
                Vector3 targetObjScale = _targetObject.transform.lossyScale;
                int scaleAxisIndex = _dragStartData.ScaleAxisIndex;

                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

                float t;
                if (_dragStartData.DragPlane.Raycast(ray, out t))
                {
                    Vector3 hitPoint = ray.GetPoint(t);
                    float distFromPivotPlane = _dragStartData.PivotPlane.GetDistanceToPoint(hitPoint);

                    float currentSize = _dragStartData.TargetOOBB.ScaledSize[_dragStartData.ScaleAxisIndex];
                    float newSize = Mathf.Abs(!_dragStartData.ScaleFromCenter ? distFromPivotPlane : 2.0f * distFromPivotPlane);
                    if (false)
                    {
                        float realNrSteps = newSize / _snapStepInWorldUnits;
                        int intSteps = (int)realNrSteps;
                        newSize = intSteps * _snapStepInWorldUnits;
                    }

                    float scaleSign = distFromPivotPlane < 0.0f ? -Mathf.Sign(_dragStartData.TargetObjectScale[scaleAxisIndex]) : Mathf.Sign(_dragStartData.TargetObjectScale[scaleAxisIndex]);
                    float scaleFactor = (newSize / currentSize);

                    targetObjScale[scaleAxisIndex] = _dragStartData.TargetObjectScale[scaleAxisIndex] * scaleFactor;
                    if (Mathf.Sign(targetObjScale[scaleAxisIndex]) != scaleSign) targetObjScale[scaleAxisIndex] *= -1.0f;
                    _targetObject.SetAbsoluteScale(targetObjScale);
                    Transform targetTransform = _targetObject.transform;
                    Vector3 scaleFactorVec = Vector3.one;
                    scaleFactorVec[scaleAxisIndex] = scaleFactor * (distFromPivotPlane < 0.0f ? -1.0f : 1.0f);

                    float prjRight = Vector3.Dot(targetTransform.right, _dragStartData.FromPivotToObjectPos);
                    float prjUp = Vector3.Dot(targetTransform.up, _dragStartData.FromPivotToObjectPos);
                    float prjLook = Vector3.Dot(targetTransform.forward, _dragStartData.FromPivotToObjectPos);


                    var npos = _dragStartData.ScalePivot +
                                                       targetTransform.right * prjRight * scaleFactorVec[0] +
                                                       targetTransform.up * prjUp * scaleFactorVec[1] +
                                                       targetTransform.forward * prjLook * scaleFactorVec[2];

                    _targetObject.transform.position = npos;
                    OnMove?.Invoke(targetObjScale, npos);
                    _objectsWereTransformedSinceLeftMouseButtonWasPressed = true;
                    UpdateTargetOOBB();
                }
            }
        }

        protected override void OnRenderObject()
        {
            //if (Camera.current != EditorCamera.Instance.Camera) return;
            base.OnRenderObject();
            if (_targetObject == null) return;

            GLPrimitives.DrawWireOOBB(_targetOOBB, _lineColor, MaterialPool.Instance.GLLine);
            foreach (var dragHandle in _dragHandles)
            {
                if (!dragHandle.IsVisible) continue;

                if (!_axesVisibilityMask[(int)dragHandle.Axis]) continue;

                Color handleColor = (_hoveredDragHandle >= 0 && dragHandle.VolumeFace == (BoxFace)_hoveredDragHandle) ? _selectedAxisColor : _axesColors[(int)dragHandle.Axis];
                GLPrimitives.Draw2DFilledRectangle(dragHandle.ScreenRect, handleColor, MaterialPool.Instance.Geometry2D, _camera);
            }
        }

        private GameObject GetTargetObject()
        {
            if (ControlledObjects == null) return null;
            List<GameObject> gameObjects = new List<GameObject>(ControlledObjects);
            if (gameObjects.Count != 1)
            {
                if (!gameObjects[0].HasMesh() && !gameObjects[0].HasBoxCollider())
                {
                    return null;
                }
            }
            return gameObjects[0];
        }

        private void UpdateTargetOOBB()
        {
            _targetOOBB = null;
            if (_targetObject != null)
            {
                _targetOOBB = _targetObject.GetWorldOrientedBox();
                _targetOOBB.Scale = _targetOOBB.Scale.GetVectorWithAbsComponents();
            }
        }

        private void UpdateDragHandles()
        {
            if (_targetOOBB == null) return;
            float halfHandleScreenSize = _dragHandleSizeInPixels * 0.5f;

            Camera camera = _camera;
            Plane cameraNearPlane = camera.GetNearPlane();
            List<BoxFace> allBoxFaces = BoxFaces.GetAll();

            foreach (BoxFace face in allBoxFaces)
            {
                Vector3 boxFaceCenter = _targetOOBB.GetBoxFaceCenter(face);
                Vector2 screenFaceCenter = camera.WorldToScreenPoint(boxFaceCenter);
                _dragHandles[(int)face].ScreenRect = new Rect(screenFaceCenter.x - halfHandleScreenSize, screenFaceCenter.y - halfHandleScreenSize, _dragHandleSizeInPixels, _dragHandleSizeInPixels);

                _dragHandles[(int)face].IsVisible = true;
                if (cameraNearPlane.GetDistanceToPoint(boxFaceCenter) < 0.0f) _dragHandles[(int)face].IsVisible = false;
            }
        }

        private int GetIndexOfHoveredDragHandle()
        {
            Vector2 devicePos = Input.mousePosition;
            //if (!InputDevice.Instance.GetPosition(out devicePos)) return -1;

            List<BoxFace> allBoxFaces = BoxFaces.GetAll();
            foreach (BoxFace face in allBoxFaces)
            {
                DragHandle dragHandle = _dragHandles[(int)face];

                if (!_axesVisibilityMask[(int)dragHandle.Axis]) continue;
                if (dragHandle.ScreenRect.Contains(devicePos, true)) return (int)face;
            }

            return -1;
        }

        private Plane CalculateDragPlane()
        {
            if (_targetOOBB == null) return new Plane();

            Matrix4x4 oobbTransform = _targetOOBB.TransformMatrix;
            Vector3 pointOnDragPlane = _targetOOBB.Center;
            Camera camera = _camera;

            Vector3 dragPlaneNormal = Vector3.zero;
            if (_dragStartData.DragHandleFace == BoxFace.Front || _dragStartData.DragHandleFace == BoxFace.Back)
            {
                var candidateNormals = new List<Vector3> { oobbTransform.GetAxis(1) };
                dragPlaneNormal = Vector3Extensions.GetMostAlignedVector(candidateNormals, camera.transform.forward);
            }
            else
            if (_dragStartData.DragHandleFace == BoxFace.Left || _dragStartData.DragHandleFace == BoxFace.Right)
            {
                var candidateNormals = new List<Vector3> { oobbTransform.GetAxis(1) };
                dragPlaneNormal = Vector3Extensions.GetMostAlignedVector(candidateNormals, camera.transform.forward);
            }
            else
            {
                var candidateNormals = new List<Vector3> { oobbTransform.GetAxis(2), oobbTransform.GetAxis(0) };
                dragPlaneNormal = Vector3Extensions.GetMostAlignedVector(candidateNormals, camera.transform.forward);
            }

            return new Plane(dragPlaneNormal, pointOnDragPlane);
        }
    }
}
