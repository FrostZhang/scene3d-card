using UnityEngine;
using UnityEditor;
using System;

namespace RTEditor
{
    /// <summary>
    /// Custom inspector implementation for the 'EditorApplication' class.
    /// </summary>
    [CustomEditor(typeof(RuntimeEditorApplication))]
    public class RuntimeEditorApplicationInspectorGUI : Editor
    {
        #region Private Variables
        /// <summary>
        /// Reference to the currently selected editor application object.
        /// </summary>
        private RuntimeEditorApplication _editorApplication;
        private static bool _xzGridSetingsAreVisible = true;

        private static bool _keyMappingsAreVisible = true;
        #endregion

        #region Public Methods
        /// <summary>
        /// Called when the inspector needs to be rendered.
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical("Box");
            bool newBool = EditorGUILayout.ToggleLeft("Enable Undo/Redo", _editorApplication.EnableUndoRedo);
            if (newBool != _editorApplication.EnableUndoRedo)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorApplication);
                _editorApplication.EnableUndoRedo = newBool;
            }

            newBool = EditorGUILayout.ToggleLeft("Use Custom Camera", _editorApplication.UseCustomCamera);
            if(newBool != _editorApplication.UseCustomCamera)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorApplication);
                _editorApplication.UseCustomCamera = newBool;
            }
            if(_editorApplication.UseCustomCamera)
            {
                Camera newCam = EditorGUILayout.ObjectField("Custom Camera", _editorApplication.CustomCamera, typeof(Camera), true) as Camera;
                if(newCam != _editorApplication.CustomCamera)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorApplication);
                    _editorApplication.CustomCamera = newCam;
                }
            }

            if (_editorApplication.UseUnityColliders) EditorGUILayout.HelpBox("Interacting with 2D sprites is not possible when this option is checked. " +
                "Uncheck this if you need to select and manipulate 2D sprites. You should only use Unity Colliders when you are experiencing low frame rates which " + 
                "can happen when working with really high res meshes.", UnityEditor.MessageType.Info);
            newBool = EditorGUILayout.ToggleLeft("Use Unity Colliders", _editorApplication.UseUnityColliders);
            if(newBool != _editorApplication.UseUnityColliders)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorApplication);
                _editorApplication.UseUnityColliders = newBool;
            }

            if(!_editorApplication.UseUnityColliders)
            {
                Vector3 newVec3 = EditorGUILayout.Vector3Field("Light Object Volume Size", _editorApplication.VolumeSizeForLightObjects);
                if (newVec3 != _editorApplication.VolumeSizeForLightObjects)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorApplication);
                    _editorApplication.VolumeSizeForLightObjects = newVec3;
                }

                newVec3 = EditorGUILayout.Vector3Field("Particle System Object Volume Size", _editorApplication.VolumeSizeForParticleSystemObjects);
                if (newVec3 != _editorApplication.VolumeSizeForParticleSystemObjects)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorApplication);
                    _editorApplication.VolumeSizeForParticleSystemObjects = newVec3;
                }

                newVec3 = EditorGUILayout.Vector3Field("Empty Object Volume Size", _editorApplication.VolumeSizeForEmptyObjects);
                if (newVec3 != _editorApplication.VolumeSizeForEmptyObjects)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorApplication);
                    _editorApplication.VolumeSizeForEmptyObjects = newVec3;
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUI.indentLevel += 1;
            EditorGUILayout.BeginVertical("Box");
            _xzGridSetingsAreVisible = EditorGUILayout.Foldout(_xzGridSetingsAreVisible, "XZ Grid Settings");
            if (_xzGridSetingsAreVisible)
            {
                EditorGUI.indentLevel += 1;
                _editorApplication.XZGrid.RenderView(_editorApplication);
                EditorGUI.indentLevel -= 1;
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel -= 1;

            _keyMappingsAreVisible = EditorGUILayout.Foldout(_keyMappingsAreVisible, "Key mappings");
            if(_keyMappingsAreVisible)
            {
                _editorApplication.ScrollGridUpDownShortcut.RenderView(_editorApplication);
                _editorApplication.ScrollGridUpDownStepShortcut.RenderView(_editorApplication);
            }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Called when the editor application object is selected in the scene view.
        /// </summary>
        protected virtual void OnEnable()
        {
            _editorApplication = target as RuntimeEditorApplication;
        }
        #endregion
    }
}
