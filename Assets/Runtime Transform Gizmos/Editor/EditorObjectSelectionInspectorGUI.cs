#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// Custom inspector implementation for the 'EditorObjectSelection' class.
    /// </summary>
    [CustomEditor(typeof(EditorObjectSelection))]
    public class EditorObjectSelectionInspectorGUI : Editor
    {       
        #region Private Static Variables
        /// <summary>
        /// The following variables control the visibility for different categories of settings.
        /// </summary>
        private static bool _objectSelectionBoxRenderSettingsAreVisible = true;
        private static bool _objectSelectionRectangleDrawSettingsAreVisible = true;
        private static bool _selectableLayersListIsVisible = true;
        private static bool _duplicatableLayersListIsVisible = true;
        private static bool _keyMappingsAreVisible = true;
        private static bool _restrictionsAreVisible = true;
        #endregion

        #region Private Variables
        /// <summary>
        /// Reference to the editor object selection module.
        /// </summary>
        private EditorObjectSelection _editorObjectSelection;
        #endregion

        #region Public Methods
        /// <summary>
        /// Called when the inspector needs to be rendered.
        /// </summary>
        public override void OnInspectorGUI()
        {
            const int indentLevel = 2;
            Color newColor;
            ObjectSelectionSettings objectSelectionSettings = _editorObjectSelection.ObjectSelectionSettings;

            EditorGUILayout.BeginVertical("Box");

            bool newBool;
            EditorGUI.indentLevel += 1;
            _restrictionsAreVisible = EditorGUILayout.Foldout(_restrictionsAreVisible, "Restrictions");
            EditorGUI.indentLevel -= 1;
            if(_restrictionsAreVisible)
            {
                EditorGUI.indentLevel += indentLevel;
                newBool = EditorGUILayout.ToggleLeft("Can Select Terrain Objects", objectSelectionSettings.CanSelectTerrainObjects);
                if (newBool != objectSelectionSettings.CanSelectTerrainObjects)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                    objectSelectionSettings.CanSelectTerrainObjects = newBool;
                }

                newBool = EditorGUILayout.ToggleLeft("Can Select Light Objects", objectSelectionSettings.CanSelectLightObjects);
                if (newBool != objectSelectionSettings.CanSelectLightObjects)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                    objectSelectionSettings.CanSelectLightObjects = newBool;
                }

                newBool = EditorGUILayout.ToggleLeft("Can Select Particle System Objects", objectSelectionSettings.CanSelectParticleSystemObjects);
                if (newBool != objectSelectionSettings.CanSelectParticleSystemObjects)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                    objectSelectionSettings.CanSelectParticleSystemObjects = newBool;
                }

                newBool = EditorGUILayout.ToggleLeft("Can Select Sprite Objects", objectSelectionSettings.CanSelectSpriteObjects);
                if (newBool != objectSelectionSettings.CanSelectSpriteObjects)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                    objectSelectionSettings.CanSelectSpriteObjects = newBool;
                }

                newBool = EditorGUILayout.ToggleLeft("Can Select Empty Objects", objectSelectionSettings.CanSelectEmptyObjects);
                if (newBool != objectSelectionSettings.CanSelectEmptyObjects)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                    objectSelectionSettings.CanSelectEmptyObjects = newBool;
                }

                newBool = EditorGUILayout.ToggleLeft("Can Click-Select", objectSelectionSettings.CanClickSelect);
                if (newBool != objectSelectionSettings.CanClickSelect)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                    objectSelectionSettings.CanClickSelect = newBool;
                }

                newBool = EditorGUILayout.ToggleLeft("Can Multi-Select", objectSelectionSettings.CanMultiSelect);
                if (newBool != objectSelectionSettings.CanMultiSelect)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                    objectSelectionSettings.CanMultiSelect = newBool;
                }
                EditorGUI.indentLevel -= indentLevel;
            }

            // Let the user specify the selectable layers
            EditorGUI.indentLevel += 1;
            _selectableLayersListIsVisible = EditorGUILayout.Foldout(_selectableLayersListIsVisible, "Selectable Layers");
            EditorGUI.indentLevel -= 1;
            if(_selectableLayersListIsVisible)
            {
                EditorGUI.indentLevel += indentLevel;

                // Show all available layer names and let the user add/remove layers using toggle buttons
                List<string> allLayerNames = LayerHelper.GetAllLayerNames();
                foreach (string layerName in allLayerNames)
                {
                    int layerNumber = LayerMask.NameToLayer(layerName);
                    bool isSelectable = LayerHelper.IsLayerBitSet(objectSelectionSettings.SelectableLayers, layerNumber);

                    newBool = EditorGUILayout.ToggleLeft(layerName, isSelectable);
                    if (newBool != isSelectable)
                    {
                        UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                        if (isSelectable) objectSelectionSettings.SelectableLayers = LayerHelper.ClearLayerBit(objectSelectionSettings.SelectableLayers, layerNumber);
                        else objectSelectionSettings.SelectableLayers = LayerHelper.SetLayerBit(objectSelectionSettings.SelectableLayers, layerNumber);
                    }
                }

                EditorGUI.indentLevel -= indentLevel;
            }

            // Let the user specify the duplicatable layers
            EditorGUI.indentLevel += 1;
            _duplicatableLayersListIsVisible = EditorGUILayout.Foldout(_duplicatableLayersListIsVisible, "Duplicatable Layers");
            EditorGUI.indentLevel -= 1;
            if (_duplicatableLayersListIsVisible)
            {
                EditorGUI.indentLevel += indentLevel;

                // Show all available layer names and let the user add/remove layers using toggle buttons
                List<string> allLayerNames = LayerHelper.GetAllLayerNames();
                foreach (string layerName in allLayerNames)
                {
                    int layerNumber = LayerMask.NameToLayer(layerName);
                    bool isDuplicatable = LayerHelper.IsLayerBitSet(objectSelectionSettings.DuplicatableLayers, layerNumber);

                    newBool = EditorGUILayout.ToggleLeft(layerName, isDuplicatable);
                    if (newBool != isDuplicatable)
                    {
                        UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                        if (isDuplicatable) objectSelectionSettings.DuplicatableLayers = LayerHelper.ClearLayerBit(objectSelectionSettings.DuplicatableLayers, layerNumber);
                        else objectSelectionSettings.DuplicatableLayers = LayerHelper.SetLayerBit(objectSelectionSettings.DuplicatableLayers, layerNumber);
                    }
                }

                EditorGUI.indentLevel -= indentLevel;
            }

            // Let the user modify the object selection box render settings
            EditorGUI.indentLevel += 1;
            _objectSelectionBoxRenderSettingsAreVisible = EditorGUILayout.Foldout(_objectSelectionBoxRenderSettingsAreVisible, "Selection Box Render Settings");
            EditorGUI.indentLevel -= 1;
            if(_objectSelectionBoxRenderSettingsAreVisible)
            {
                EditorGUI.indentLevel += indentLevel;
                ObjectSelectionBoxRenderSettings objectSelectionBoxDrawSettings = objectSelectionSettings.ObjectSelectionBoxRenderSettings;

                newBool = EditorGUILayout.ToggleLeft("Draw Selection Boxes", objectSelectionBoxDrawSettings.DrawBoxes);
                if(newBool != objectSelectionBoxDrawSettings.DrawBoxes)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                    objectSelectionBoxDrawSettings.DrawBoxes = newBool;
                }

                // Let the user choose the object selection box style
                ObjectSelectionBoxStyle newObjectSelectionBoxStyle = (ObjectSelectionBoxStyle)EditorGUILayout.EnumPopup("Selection Box Style", objectSelectionBoxDrawSettings.SelectionBoxStyle);
                if(newObjectSelectionBoxStyle != objectSelectionBoxDrawSettings.SelectionBoxStyle)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                    objectSelectionBoxDrawSettings.SelectionBoxStyle = newObjectSelectionBoxStyle;
                }

                ObjectSelectionBoxRenderMode newObjSelBoxRenderMode = (ObjectSelectionBoxRenderMode)EditorGUILayout.EnumPopup("Selection Box Render Mode", objectSelectionBoxDrawSettings.SelectionBoxRenderMode);
                if (newObjSelBoxRenderMode != objectSelectionBoxDrawSettings.SelectionBoxRenderMode)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                    objectSelectionBoxDrawSettings.SelectionBoxRenderMode = newObjSelBoxRenderMode;
                }

                // If the object selection box style is set to 'CornerLines', let the user choose the length of the corner lines
                float newFloatValue;
                if(objectSelectionBoxDrawSettings.SelectionBoxStyle == ObjectSelectionBoxStyle.CornerLines)
                {
                    newFloatValue = EditorGUILayout.FloatField("Corner Line Percentage", objectSelectionBoxDrawSettings.SelectionBoxCornerLinePercentage);
                    if(newFloatValue != objectSelectionBoxDrawSettings.SelectionBoxCornerLinePercentage)
                    {
                        UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                        objectSelectionBoxDrawSettings.SelectionBoxCornerLinePercentage = newFloatValue;
                    }
                }

                // Let the user choose the selection box line color
                Color newColorValue = EditorGUILayout.ColorField("Selection Box Line Color", objectSelectionBoxDrawSettings.SelectionBoxLineColor);
                if(newColorValue != objectSelectionBoxDrawSettings.SelectionBoxLineColor)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                    objectSelectionBoxDrawSettings.SelectionBoxLineColor = newColorValue;
                }

                // Let the user choose the selection box size add value
                newFloatValue = EditorGUILayout.FloatField("Selection Box Size Add", objectSelectionBoxDrawSettings.BoxSizeAdd);
                if(newFloatValue != objectSelectionBoxDrawSettings.BoxSizeAdd)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                    objectSelectionBoxDrawSettings.BoxSizeAdd = newFloatValue;
                }
                EditorGUI.indentLevel -= indentLevel;
            }

            // Let the user modify the object selection rectangle render settings    
            EditorGUI.indentLevel += 1;
            _objectSelectionRectangleDrawSettingsAreVisible = EditorGUILayout.Foldout(_objectSelectionRectangleDrawSettingsAreVisible, "Selection Rectangle Render Settings");
            EditorGUI.indentLevel -= 1;
            if(_objectSelectionRectangleDrawSettingsAreVisible)
            {
                EditorGUI.indentLevel += indentLevel;

                // Let the user modify the object selection border line color
                ObjectSelectionRectangleRenderSettings objectSelectionRectangleDrawSettings = _editorObjectSelection.ObjectSelectionRectangleRenderSettings;
                newColor = EditorGUILayout.ColorField("Border Line Color", objectSelectionRectangleDrawSettings.BorderLineColor);
                if(newColor != objectSelectionRectangleDrawSettings.BorderLineColor)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                    objectSelectionRectangleDrawSettings.BorderLineColor = newColor;
                }

                // Let the user modify the object selection rectangle fill color
                newColor = EditorGUILayout.ColorField("Fill Color", objectSelectionRectangleDrawSettings.FillColor);
                if(newColor != objectSelectionRectangleDrawSettings.FillColor)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                    objectSelectionRectangleDrawSettings.FillColor = newColor;
                }

                EditorGUI.indentLevel -= indentLevel;
            }

            EditorGUILayout.EndVertical();

            _keyMappingsAreVisible = EditorGUILayout.Foldout(_keyMappingsAreVisible, "Key mappings");
            if (_keyMappingsAreVisible)
            {
                _editorObjectSelection.AppendToSelectionShortcut.RenderView(_editorObjectSelection);
                _editorObjectSelection.MultiDeselectShortcut.RenderView(_editorObjectSelection);
                _editorObjectSelection.DuplicateSelectionShortcut.RenderView(_editorObjectSelection);
                _editorObjectSelection.DeleteSelectionShortcut.RenderView(_editorObjectSelection);
            }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Called when the editor object selection object is selected in the scene view.
        /// </summary>
        protected virtual void OnEnable()
        {
            _editorObjectSelection = target as EditorObjectSelection;
        }
        #endregion
    }
}
#endif
