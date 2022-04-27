#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace RTEditor
{
    [CustomEditor(typeof(VolumeScaleGizmo))]
    public class VolumeScaleGizmoInspectorGUI : GizmoInspectorGUIBase
    {
        private static bool _keyMappingsAreVisible = true;
        private VolumeScaleGizmo _volumeScaleGizmo;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUILayout.BeginVertical("Box");
            Color newColor = EditorGUILayout.ColorField("Line Color", _volumeScaleGizmo.LineColor);
            if(newColor != _volumeScaleGizmo.LineColor)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_volumeScaleGizmo);
                _volumeScaleGizmo.LineColor = newColor;
            }

            int newInt = EditorGUILayout.IntField("Drag Handle Size (in pixels)", _volumeScaleGizmo.DragHandleSizeInPixels);
            if(newInt != _volumeScaleGizmo.DragHandleSizeInPixels)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_volumeScaleGizmo);
                _volumeScaleGizmo.DragHandleSizeInPixels = newInt;
            }

            EditorGUILayout.Separator();
            float newFloat = EditorGUILayout.FloatField("Snap Step (In World Units)", _volumeScaleGizmo.SnapStepInWorldUnits);
            if (newFloat != _volumeScaleGizmo.SnapStepInWorldUnits)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_volumeScaleGizmo);
                _volumeScaleGizmo.SnapStepInWorldUnits = newFloat;
            }
            EditorGUILayout.EndVertical();

            _keyMappingsAreVisible = EditorGUILayout.Foldout(_keyMappingsAreVisible, "Key mappings");
            if (_keyMappingsAreVisible)
            {
                _volumeScaleGizmo.EnableScaleFromCenterShortcut.RenderView(_volumeScaleGizmo);
                _volumeScaleGizmo.EnableStepSnappingShortcut.RenderView(_volumeScaleGizmo);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _volumeScaleGizmo = target as VolumeScaleGizmo;
        }
    }
}
#endif