using UnityEngine;
using UnityEditor;

namespace RTEditor
{
    [CustomEditor(typeof(SceneGizmo))]
    public class SceneGizmoInspectorGUI : Editor
    {
        #region Private Variables
        private SceneGizmo _sceneGizmo;
        #endregion

        #region Public Methods
        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical("Box");
            SceneGizmoCorner newCorner = (SceneGizmoCorner)EditorGUILayout.EnumPopup("Corner", _sceneGizmo.Corner);
            if (newCorner != _sceneGizmo.Corner)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_sceneGizmo);
                _sceneGizmo.Corner = newCorner;
            }

            Color newColor = EditorGUILayout.ColorField("X Axis Clolor", _sceneGizmo.XAxisColor);
            if(newColor != _sceneGizmo.XAxisColor)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_sceneGizmo);
                _sceneGizmo.XAxisColor = newColor;
            }

            newColor = EditorGUILayout.ColorField("Y Axis Clolor", _sceneGizmo.YAxisColor);
            if (newColor != _sceneGizmo.YAxisColor)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_sceneGizmo);
                _sceneGizmo.YAxisColor = newColor;
            }

            newColor = EditorGUILayout.ColorField("Z Axis Clolor", _sceneGizmo.ZAxisColor);
            if (newColor != _sceneGizmo.ZAxisColor)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_sceneGizmo);
                _sceneGizmo.ZAxisColor = newColor;
            }

            newColor = EditorGUILayout.ColorField("Negative Axis Color", _sceneGizmo.NegativeAxisColor);
            if (newColor != _sceneGizmo.NegativeAxisColor)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_sceneGizmo);
                _sceneGizmo.NegativeAxisColor = newColor;
            }

            newColor = EditorGUILayout.ColorField("Cube Color", _sceneGizmo.CubeColor);
            if (newColor != _sceneGizmo.CubeColor)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_sceneGizmo);
                _sceneGizmo.CubeColor = newColor;
            }

            newColor = EditorGUILayout.ColorField("Hovered Component Color", _sceneGizmo.HoveredComponentColor);
            if (newColor != _sceneGizmo.HoveredComponentColor)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_sceneGizmo);
                _sceneGizmo.HoveredComponentColor = newColor;
            }

            float newFloat = EditorGUILayout.FloatField("Camera Look Align Duration (seconds)", _sceneGizmo.CameraAlignDuration);
            if(newFloat != _sceneGizmo.CameraAlignDuration)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_sceneGizmo);
                _sceneGizmo.CameraAlignDuration = newFloat;
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Protected Methods
        protected virtual void OnEnable()
        {
            _sceneGizmo = target as SceneGizmo;
        }
        #endregion
    }
}
