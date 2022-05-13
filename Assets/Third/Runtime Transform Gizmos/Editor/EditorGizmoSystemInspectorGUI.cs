#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// Custom inspector implementation for the 'EditorGizmoSystem' class.
    /// </summary>
    [CustomEditor(typeof(EditorGizmoSystem))]
    public class EditorGizmoSystemInspectorGUI : Editor
    {
        private static bool _keyMappingsAreVisible = true;

        #region Private Variables
        /// <summary>
        /// Reference to the currently selected gizmo system object.
        /// </summary>
        private EditorGizmoSystem _gizmoSystem;
        #endregion

        #region Public Methods
        /// <summary>
        /// Called when the inspector needs to be rendered.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Let the user specify the translation gizmo
            EditorGUILayout.BeginVertical("Box");
            TranslationGizmo translationGizmo = EditorGUILayout.ObjectField("Translation Gizmo", _gizmoSystem.TranslationGizmo, typeof(TranslationGizmo), true) as TranslationGizmo;
            if (translationGizmo != _gizmoSystem.TranslationGizmo)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_gizmoSystem);
                _gizmoSystem.TranslationGizmo = translationGizmo;
            }

            // Let the user specify the rotation gizmo
            RotationGizmo rotationGizmo = EditorGUILayout.ObjectField("Rotation Gizmo", _gizmoSystem.RotationGizmo, typeof(RotationGizmo), true) as RotationGizmo;
            if (rotationGizmo != _gizmoSystem.RotationGizmo)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_gizmoSystem);
                _gizmoSystem.RotationGizmo = rotationGizmo;
            }

            // Let the user specify the scale gizmo
            ScaleGizmo scaleGizmo = EditorGUILayout.ObjectField("Scale Gizmo", _gizmoSystem.ScaleGizmo, typeof(ScaleGizmo), true) as ScaleGizmo;
            if (scaleGizmo != _gizmoSystem.ScaleGizmo)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_gizmoSystem);
                _gizmoSystem.ScaleGizmo = scaleGizmo;
            }

            VolumeScaleGizmo volumeScaleGizmo = EditorGUILayout.ObjectField("Volume Scale Gizmo", _gizmoSystem.VolumeScaleGizmo, typeof(VolumeScaleGizmo), true) as VolumeScaleGizmo;
            if (scaleGizmo != _gizmoSystem.VolumeScaleGizmo)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_gizmoSystem);
                _gizmoSystem.VolumeScaleGizmo = volumeScaleGizmo;
            }

            // Let the user specify the active gimo type
            if(_gizmoSystem.IsAnyGizmoTypeAvailable())
            {
                EditorGUILayout.Separator();
                GizmoType newActiveGizmoType = (GizmoType)EditorGUILayout.EnumPopup("Active Gizmo Type", _gizmoSystem.ActiveGizmoType);
                if (newActiveGizmoType != _gizmoSystem.ActiveGizmoType)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_gizmoSystem);
                    _gizmoSystem.ActiveGizmoType = newActiveGizmoType;
                }
            }

            EditorGUILayout.Separator();
            List<GizmoType> allGizmoTypes = Enum.GetValues(typeof(GizmoType)).Cast<GizmoType>().ToList();
            foreach (var gizmoType in allGizmoTypes)
            {
                bool isGizmoTypeAvailable = _gizmoSystem.IsGizmoTypeAvailable(gizmoType);
                bool newBool = EditorGUILayout.ToggleLeft(gizmoType.ToString(), isGizmoTypeAvailable);
                if (isGizmoTypeAvailable != newBool)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_gizmoSystem);
                    if (newBool) _gizmoSystem.SetGizmoTypeAvailable(gizmoType, true);
                    else _gizmoSystem.SetGizmoTypeAvailable(gizmoType, false);
                }
            }
            EditorGUILayout.EndVertical();

            _keyMappingsAreVisible = EditorGUILayout.Foldout(_keyMappingsAreVisible, "Key mappings");
            if(_keyMappingsAreVisible)
            {
                _gizmoSystem.ActivateTranslationGizmoShortcut.RenderView(_gizmoSystem);
                _gizmoSystem.ActivateRotationGizmoShortcut.RenderView(_gizmoSystem);
                _gizmoSystem.ActivateScaleGizmoShortcut.RenderView(_gizmoSystem);
                _gizmoSystem.ActivateVolumeScaleGizmoShortcut.RenderView(_gizmoSystem);
                _gizmoSystem.ActivateGlobalTransformShortcut.RenderView(_gizmoSystem);
                _gizmoSystem.ActivateLocalTransformShortcut.RenderView(_gizmoSystem);
                _gizmoSystem.TurnOffGizmosShortcut.RenderView(_gizmoSystem);
                _gizmoSystem.TogglePivotShortcut.RenderView(_gizmoSystem);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Called when the gizmo system object is selected in the scene view.
        /// </summary>
        private void OnEnable()
        {
            _gizmoSystem = target as EditorGizmoSystem;
        }
        #endregion
    }
}
#endif