using UnityEngine;

namespace RTEditor
{
    public class ObjectSelectEventArgs
    {
        private ObjectSelectActionType _selectActionType;
        private GizmoType _gizmoType;
        private bool _isGizmoActive;

        public ObjectSelectActionType SelectActionType { get { return _selectActionType; } }
        public GizmoType GizmoType { get { return _gizmoType; } }
        public bool IsGizmoActive { get { return _isGizmoActive; } }

        public ObjectSelectEventArgs(ObjectSelectActionType selectActionType)
        {
            _selectActionType = selectActionType;
            _gizmoType = EditorGizmoSystem.Instance.ActiveGizmoType;
            _isGizmoActive = !EditorGizmoSystem.Instance.AreGizmosTurnedOff;
        }
    }
}
