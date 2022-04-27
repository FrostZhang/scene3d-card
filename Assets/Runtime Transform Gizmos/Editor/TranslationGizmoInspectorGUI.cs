#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// Custom inspector implementation for the 'TranslationGizmo' class.
    /// </summary>
    [CustomEditor(typeof(TranslationGizmo))]
    public class TranslationGizmoInspectorGUI : GizmoInspectorGUIBase
    {
        private static bool _keyMappingsAreVisible = true;
        private static bool _vertexSnapLayersVisible = true;

        #region Private Variables
        /// <summary>
        /// Reference to the currently selected translation gizmo.
        /// </summary>
        private TranslationGizmo _translationGizmo;
        #endregion

        #region Public Methods
        /// <summary>
        /// Called when the inspector needs to be rendered.
        /// </summary>
        public override void OnInspectorGUI()
        {
            bool newBool;

            // Draw the common gizmo properties
            base.OnInspectorGUI();

            EditorGUILayout.BeginVertical("Box");

            // Let the user control the gizmo axis length
            float newFloatValue = EditorGUILayout.FloatField("Axis Length", _translationGizmo.AxisLength);
            if (newFloatValue != _translationGizmo.AxisLength)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
                _translationGizmo.AxisLength = newFloatValue;
            }

            // Let the user control the radius of the arrow cones which sit at the tip of each axis
            newFloatValue = EditorGUILayout.FloatField("Arrow Cone Radius", _translationGizmo.ArrowConeRadius);
            if (newFloatValue != _translationGizmo.ArrowConeRadius)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
                _translationGizmo.ArrowConeRadius = newFloatValue;
            }

            // Let the user control the length of the arrow cones which sit at the tip of each axis
            newFloatValue = EditorGUILayout.FloatField("Arrow Cone Length", _translationGizmo.ArrowConeLength);
            if (newFloatValue != _translationGizmo.ArrowConeLength)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
                _translationGizmo.ArrowConeLength = newFloatValue;
            }

            // Let the user specify whether or not the arrow cones must be lit
            bool newBoolValue = EditorGUILayout.ToggleLeft("Are Arrow Cones Lit", _translationGizmo.AreArrowConesLit);
            if (newBoolValue != _translationGizmo.AreArrowConesLit)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
                _translationGizmo.AreArrowConesLit = newBoolValue;
            }

            EditorGUILayout.Separator();
            newFloatValue = EditorGUILayout.FloatField("Multi Axis Square Alpha", _translationGizmo.MultiAxisSquareAlpha);
            if (newFloatValue != _translationGizmo.MultiAxisSquareAlpha)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
                _translationGizmo.MultiAxisSquareAlpha = newFloatValue;
            }

            // Let the user change the multi-axis square size
            newFloatValue = EditorGUILayout.FloatField("Multi Axis Square Size", _translationGizmo.MultiAxisSquareSize);
            if (newFloatValue != _translationGizmo.MultiAxisSquareSize)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
                _translationGizmo.MultiAxisSquareSize = newFloatValue;
            }

            // Let the user specify whether or not the mulit-axis squares must be adjusted during runtime for better visibility
            newBoolValue = EditorGUILayout.ToggleLeft("Adjust Multi Axis For Better Visibility", _translationGizmo.AdjustMultiAxisForBetterVisibility);
            if (newBoolValue != _translationGizmo.AdjustMultiAxisForBetterVisibility)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
                _translationGizmo.AdjustMultiAxisForBetterVisibility = newBoolValue;
            }

            // Let the user specify the special op square line color
            EditorGUILayout.Separator();
            Color newColorValue = EditorGUILayout.ColorField("Color Of Special Op Square", _translationGizmo.SpecialOpSquareColor);
            if (newColorValue != _translationGizmo.SpecialOpSquareColor)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
                _translationGizmo.SpecialOpSquareColor = newColorValue;
            }

            // Let the user specify the special op square line color when the square is selected
            newColorValue = EditorGUILayout.ColorField("Color Of Special Op Square (Selected)", _translationGizmo.SpecialOpSquareColorWhenSelected);
            if (newColorValue != _translationGizmo.SpecialOpSquareColorWhenSelected)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
                _translationGizmo.SpecialOpSquareColorWhenSelected = newColorValue;
            }

            // Let the user specify the screen size of the special op square
            newFloatValue = EditorGUILayout.FloatField("Screen Size Of Special Op Square", _translationGizmo.ScreenSizeOfSpecialOpSquare);
            if (newFloatValue != _translationGizmo.ScreenSizeOfSpecialOpSquare)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
                _translationGizmo.ScreenSizeOfSpecialOpSquare = newFloatValue;
            }

            // Let the user specify the snap step value
            EditorGUILayout.Separator();
            newFloatValue = EditorGUILayout.FloatField("Snap Step Value (In World Units)", _translationGizmo.SnapSettings.StepValueInWorldUnits);
            if (newFloatValue != _translationGizmo.SnapSettings.StepValueInWorldUnits)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
                _translationGizmo.SnapSettings.StepValueInWorldUnits = newFloatValue;
            }

            newFloatValue = EditorGUILayout.FloatField("Move Scale (When active)", _translationGizmo.MoveScale);
            if (newFloatValue != _translationGizmo.MoveScale)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
                _translationGizmo.MoveScale = newFloatValue;
            }
            EditorGUILayout.EndVertical();

            RenderLayerMaskControls();
            EditorGUI.indentLevel += 1;
            _vertexSnapLayersVisible = EditorGUILayout.Foldout(_vertexSnapLayersVisible, "Vertex Snap Layers");
            EditorGUI.indentLevel -= 1;
            if (_vertexSnapLayersVisible)
            {
                EditorGUILayout.BeginVertical("Box");
                List<string> allLayerNames = LayerHelper.GetAllLayerNames();
                foreach (string layerName in allLayerNames)
                {
                    int layerNumber = LayerMask.NameToLayer(layerName);
                    bool isBitSet = _translationGizmo.IsVertexSnapLayerBitSet(layerNumber);

                    newBool = EditorGUILayout.ToggleLeft(layerName, isBitSet);
                    if (newBool != isBitSet)
                    {
                        UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
                        if (isBitSet) _translationGizmo.SetVertexSnapLayerBit(layerNumber, false);
                        else _translationGizmo.SetVertexSnapLayerBit(layerNumber, true);
                    }
                }

                EditorGUILayout.EndVertical();
            }

            _keyMappingsAreVisible = EditorGUILayout.Foldout(_keyMappingsAreVisible, "Key mappings");
            if (_keyMappingsAreVisible)
            {
                _translationGizmo.TranslateAlongScreenAxesShortcut.RenderView(_translationGizmo);
                _translationGizmo.EnableStepSnappingShortcut.RenderView(_translationGizmo);
                _translationGizmo.EnableVertexSnappingShortcut.RenderView(_translationGizmo);
                _translationGizmo.EnableBoxSnappingShortcut.RenderView(_translationGizmo);
                _translationGizmo.EnableSurfacePlacementWithXAlignment.RenderView(_translationGizmo);
                _translationGizmo.EnableSurfacePlacementWithYAlignment.RenderView(_translationGizmo);
                _translationGizmo.EnableSurfacePlacementWithZAlignment.RenderView(_translationGizmo);
                _translationGizmo.EnableSurfacePlacementWithNoAxisAlignment.RenderView(_translationGizmo);
                _translationGizmo.EnableMoveScale.RenderView(_translationGizmo);
            }

            // Make sure that if any color properites have been modified, the changes can be seen immediately in the scene view
            SceneView.RepaintAll();
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Called when the gizmo is selected in the scene view.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            _translationGizmo = target as TranslationGizmo;
        }
        #endregion
    }
}
#endif
