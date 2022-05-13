#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// Custom inspector implementation for the 'Gizmo' base class.
    /// </summary>
    [CustomEditor(typeof(Gizmo))]
    public class GizmoInspectorGUIBase : Editor
    {
        private static bool _transformableLayersListIsVisible = true;

        #region Private Variables
        /// <summary>
        /// Reference to the currently selected gizmo.
        /// </summary>
        private Gizmo _gizmo;
        #endregion

        #region Public Methods
        /// <summary>
        /// Called when the inspector needs to be rendered.
        /// </summary>
        public override void OnInspectorGUI()
        {
            float newFloatValue;
            bool newBoolValue;

            EditorGUILayout.BeginVertical("Box");

            // Allow the user to specify the gizmo base scale
            if(_gizmo.GetGizmoType() != GizmoType.VolumeScale)
            {
                newFloatValue = EditorGUILayout.FloatField("Gizmo Base Scale", _gizmo.GizmoBaseScale);
                if (newFloatValue != _gizmo.GizmoBaseScale)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_gizmo);
                    _gizmo.GizmoBaseScale = newFloatValue;
                }

                // Allow the user to specify if whether or not the size of the gizmo must be preserved in screen space
                newBoolValue = EditorGUILayout.ToggleLeft("Preserve Gizmo Screen Size", _gizmo.PreserveGizmoScreenSize);
                if (newBoolValue != _gizmo.PreserveGizmoScreenSize)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_gizmo);
                    _gizmo.PreserveGizmoScreenSize = newBoolValue;
                }
                EditorGUILayout.Separator();
            }

            // Loop through each axis and let the user modify their colors
            for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
            {
                // Construct the text used to draw the axis label
                string axisLabelText = ((GizmoAxis)axisIndex).ToString() + " Axis Color";

                // Allow the user to change the color
                Color currentAxisColor = _gizmo.GetAxisColor((GizmoAxis)axisIndex);
                Color newAxisColor = EditorGUILayout.ColorField(axisLabelText, currentAxisColor);
                if (newAxisColor != currentAxisColor)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_gizmo);
                    _gizmo.SetAxisColor((GizmoAxis)axisIndex, newAxisColor);
                }
            }

            // Allow the user to choose the color which must be used to draw the currently selected axis
            Color newColorValue = EditorGUILayout.ColorField("Selected Axis Color", _gizmo.SelectedAxisColor);
            if (newColorValue != _gizmo.SelectedAxisColor)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_gizmo);
                _gizmo.SelectedAxisColor = newColorValue;
            }

            EditorGUILayout.BeginHorizontal();
            string[] axesLabels = new string[] { "X", "Y", "Z"};
            for(int axisIndex = 0; axisIndex < 3; ++axisIndex)
            {
                bool isAxisVisible = _gizmo.GetAxisVisibility(axisIndex);
                newBoolValue = EditorGUILayout.ToggleLeft("Show " + axesLabels[axisIndex] + " Axis", isAxisVisible, GUILayout.Width(90.0f));
                if (newBoolValue != isAxisVisible)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_gizmo);
                    _gizmo.SetAxisVisibility(newBoolValue, axisIndex);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            // Make sure that if any color properites have been modified, the changes can be seen immediately in the scene view
            SceneView.RepaintAll();
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Called when the gizmo is selected in the scene view.
        /// </summary>
        protected virtual void OnEnable()
        {
            _gizmo = target as Gizmo;
        }

        protected void RenderLayerMaskControls()
        {
            bool newBool;

            // Let the user specify which layers can be transformed by the gizmo
            EditorGUI.indentLevel += 1;
            _transformableLayersListIsVisible = EditorGUILayout.Foldout(_transformableLayersListIsVisible, "Transformable Layers");
            EditorGUI.indentLevel -= 1;
            if (_transformableLayersListIsVisible)
            {
                EditorGUILayout.BeginVertical("Box");

                // Show all available layer names and let the user add/remove layers using toggle buttons
                List<string> allLayerNames = LayerHelper.GetAllLayerNames();
                foreach (string layerName in allLayerNames)
                {
                    int layerNumber = LayerMask.NameToLayer(layerName);
                    bool isTransformable = !_gizmo.IsObjectLayerMasked(layerNumber);

                    newBool = EditorGUILayout.ToggleLeft(layerName, isTransformable);
                    if (newBool != isTransformable)
                    {
                        UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_gizmo);
                        if (isTransformable) _gizmo.MaskObjectLayer(layerNumber);
                        else _gizmo.UnmaskObjectkLayer(layerNumber);
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }
        #endregion
    }
}
#endif
